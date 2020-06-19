using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//enum utworzony żeby ułatwić odwoływanie się do tablic markerów
//poszczególne wartości odwołują się do poszczególnych markerów
//UL - upper left
//UR - upper right
//BL - bottom left
//BR - bottom right

//aby odwołać się trzeba zrzutować enum na inta 
//np. tablica[(int)d.UL]
enum D{ UL=0,UR=1,BL=2,BR=3}

//enum utworzony żeby zwiększyć czytelność kodu i na potrzeby maszyny stanów
enum GeneratorState { Stopped=0,Trail=1, Gap=2, Replacing=3}

//klasa generująca ślad gracza na zasadach podobnych jak w grze Achtung die kurve
//ślad jest generowany ciągle i nie przesuwa się za graczem natomiast generowane są przerwy co określoną długość śladu
//generowany ślad jest obiektem typu mesh
//korzystamy z korutyn dla zwiększenia możliwości zarządzania czasem podczas uruchomienia generatora
public class TrailGenerator : MonoBehaviour
{
    private float currDistance;
    private GeneratorState currentState;
    private Mesh mesh;
    private MeshCollider meshCollider; 
    private Transform[] headTransforms; //aktualne współrzędne markerów
    private Vector3[] lastPoints; //poprzednie współrzędne markerów
    private List<Vector3> vertices; //współrzędne wierzchołków
    private List<int> triangles; //numeracja wierzchołków
    private int currTriangleNo; //aktualny numer wierzchołka trójkąta (rośnie o 1 przy tworzeniu vertice)
    [SerializeField] private GameObject player;
    [SerializeField] private float timeToNextUpdate = 0.05f; //jak często ma być odświeżana korutyna
    [SerializeField] private float trailDistance = 10f; //jak długi ma być kawałek ogona przed następną przerwą
    [SerializeField] private float gapDistance = 1f; //jak długa ma być przerwa przed następnym kawałkiem ogona
    private bool isGenerating; //zmienna przechowująca informacje czy ma generować scieżkę czy nie
    private Coroutine trailGenerationCoroutine; //zmienna przechowująca korutynę, potrzebna aby ją zatrzymać

    //DEPRECATED
    private float distanceTreshold = 0.5f; //jak duży ma byc dystans pomiędzy starą lokacją a nową żeby stwierdzić że gracz się rusza

    //przed funkcją Start
    private void Awake()
    {
        //inicjalizacja zmiennych
        currentState = GeneratorState.Trail;
        isGenerating = true;
        mesh = GetComponent<MeshFilter>().mesh;
        meshCollider = GetComponent<MeshCollider>();
        headTransforms = new Transform[4];
        lastPoints = new Vector3[4];
        currTriangleNo = -1;
        //wyszukanie a następnie przypisanie markerów mesha w dzieciach obiektu
        Transform mp = player.transform.Find("HeadObject").transform.Find("MeshPoints");
        headTransforms[(int)D.UL] = mp.Find("MeshPointUL").transform;
        headTransforms[(int)D.UR] = mp.Find("MeshPointUR").transform;
        headTransforms[(int)D.BL] = mp.Find("MeshPointBL").transform;
        headTransforms[(int)D.BR] = mp.Find("MeshPointBR").transform;

        vertices = new List<Vector3>();
        triangles = new List<int>();
        
    }
    // Start is called before the first frame update
    void Start()
    {
        trailGenerationCoroutine=StartCoroutine(TrailGenerationDistance());
    }

    //funkcja dodająca nowe trójkaty do tablic verices i triangles
    void GenerateTrailPart()
    {
        if (isGenerating)
        {
            //lewa ścianka
            {
                //górny trójkąt lewa ścianka
                vertices.Add(this.transform.InverseTransformPoint(headTransforms[(int)D.UL].position));
                triangles.Add(currTriangleNo += 1);
                vertices.Add(this.transform.InverseTransformPoint(lastPoints[(int)D.UL]));
                triangles.Add(currTriangleNo += 1);
                vertices.Add(this.transform.InverseTransformPoint(lastPoints[(int)D.BL]));
                triangles.Add(currTriangleNo += 1);


                //dolny trójkąt lewa ścianka
                vertices.Add(this.transform.InverseTransformPoint(headTransforms[(int)D.UL].position));
                triangles.Add(currTriangleNo += 1);
                vertices.Add(this.transform.InverseTransformPoint(lastPoints[(int)D.BL]));
                triangles.Add(currTriangleNo += 1);
                vertices.Add(this.transform.InverseTransformPoint(headTransforms[(int)D.BL].position));
                triangles.Add(currTriangleNo += 1);
            }

            //prawa ścianka
            {
                //górny trójkąt prawa ścianka
                vertices.Add(this.transform.InverseTransformPoint(lastPoints[(int)D.BR]));
                triangles.Add(currTriangleNo += 1);
                vertices.Add(this.transform.InverseTransformPoint(lastPoints[(int)D.UR]));
                triangles.Add(currTriangleNo += 1);
                vertices.Add(this.transform.InverseTransformPoint(headTransforms[(int)D.UR].position));
                triangles.Add(currTriangleNo += 1);

                //dolny trójkąt prawa ścianka
                vertices.Add(this.transform.InverseTransformPoint(headTransforms[(int)D.BR].position));
                triangles.Add(currTriangleNo += 1);
                vertices.Add(this.transform.InverseTransformPoint(lastPoints[(int)D.BR]));
                triangles.Add(currTriangleNo += 1);
                vertices.Add(this.transform.InverseTransformPoint(headTransforms[(int)D.UR].position));
                triangles.Add(currTriangleNo += 1);
            }

            //górna ścianka
            {
                //lewy trójkąt górna ścianka
                vertices.Add(this.transform.InverseTransformPoint(headTransforms[(int)D.UL].position));
                triangles.Add(currTriangleNo += 1);
                vertices.Add(this.transform.InverseTransformPoint(headTransforms[(int)D.UR].position));
                triangles.Add(currTriangleNo += 1);
                vertices.Add(this.transform.InverseTransformPoint(lastPoints[(int)D.UL]));
                triangles.Add(currTriangleNo += 1);


                //prawy trójkąt górna ścianka
                vertices.Add(this.transform.InverseTransformPoint(headTransforms[(int)D.UR].position));
                triangles.Add(currTriangleNo += 1);
                vertices.Add(this.transform.InverseTransformPoint(lastPoints[(int)D.UR]));
                triangles.Add(currTriangleNo += 1);
                vertices.Add(this.transform.InverseTransformPoint(lastPoints[(int)D.UL]));
                triangles.Add(currTriangleNo += 1);
            }

            //dolna ścianka
            {
                //lewy trójkąt dolna ścianka
                vertices.Add(this.transform.InverseTransformPoint(lastPoints[(int)D.BL]));
                triangles.Add(currTriangleNo += 1);
                vertices.Add(this.transform.InverseTransformPoint(headTransforms[(int)D.BR].position));
                triangles.Add(currTriangleNo += 1);
                vertices.Add(this.transform.InverseTransformPoint(headTransforms[(int)D.BL].position));
                triangles.Add(currTriangleNo += 1);


                //prawy trójkąt dolna ścianka
                vertices.Add(this.transform.InverseTransformPoint(lastPoints[(int)D.BL]));
                triangles.Add(currTriangleNo += 1);
                vertices.Add(this.transform.InverseTransformPoint(lastPoints[(int)D.BR]));
                triangles.Add(currTriangleNo += 1);
                vertices.Add(this.transform.InverseTransformPoint(headTransforms[(int)D.BR].position));
                triangles.Add(currTriangleNo += 1);
            }
            UpdateMesh();
        }
        
    }

    //funkcja czyszcząca starego mesha i generująca nowego mesha na podstawie aktualnych tablic
    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.Optimize();
        mesh.RecalculateNormals();
        meshCollider.sharedMesh = mesh;
      
    }

    //funkcja przypisująca aktualne wartości do tablicy lastPoints
    private void SetLastPoints()
    {
        lastPoints[(int)D.UL] = headTransforms[(int)D.UL].position;
        lastPoints[(int)D.UR] = headTransforms[(int)D.UR].position;
        lastPoints[(int)D.BL] = headTransforms[(int)D.BL].position;
        lastPoints[(int)D.BR] = headTransforms[(int)D.BR].position;
    }

    
    //funkcja tworząca przednią ściankę ogona
    private void GenerateFirstPlane()
    {
        vertices.Add(this.transform.InverseTransformPoint(headTransforms[(int)D.UL].position));
        triangles.Add(currTriangleNo += 1);
        vertices.Add(this.transform.InverseTransformPoint(headTransforms[(int)D.UR].position));
        triangles.Add(currTriangleNo += 1);
        vertices.Add(this.transform.InverseTransformPoint(headTransforms[(int)D.BL].position));
        triangles.Add(currTriangleNo += 1);
        vertices.Add(this.transform.InverseTransformPoint(headTransforms[(int)D.UR].position));
        triangles.Add(currTriangleNo += 1);
        vertices.Add(this.transform.InverseTransformPoint(headTransforms[(int)D.BR].position));
        triangles.Add(currTriangleNo += 1);
        vertices.Add(this.transform.InverseTransformPoint(headTransforms[(int)D.BL].position));
        triangles.Add(currTriangleNo += 1);
        UpdateMesh();
    }

    //funkcja tworząca tylną ściankę ogona
    private void GenerateLastPlane()
    {
        vertices.Add(this.transform.InverseTransformPoint(lastPoints[(int)D.BL]));
        triangles.Add(currTriangleNo += 1);
        vertices.Add(this.transform.InverseTransformPoint(lastPoints[(int)D.UR]));
        triangles.Add(currTriangleNo += 1);
        vertices.Add(this.transform.InverseTransformPoint(lastPoints[(int)D.UL]));
        triangles.Add(currTriangleNo += 1);
        vertices.Add(this.transform.InverseTransformPoint(lastPoints[(int)D.BL]));
        triangles.Add(currTriangleNo += 1);
        vertices.Add(this.transform.InverseTransformPoint(lastPoints[(int)D.BR]));
        triangles.Add(currTriangleNo += 1);
        vertices.Add(this.transform.InverseTransformPoint(lastPoints[(int)D.UR]));
        triangles.Add(currTriangleNo += 1);
        UpdateMesh();
    }

    //główna korutyna generująca ogon gracza
    IEnumerator TrailGenerationDistance()
    {
        //generowanie pierwszego przedniego kawałka
        GenerateFirstPlane();
        SetLastPoints();
        yield return new WaitForSeconds(timeToNextUpdate);
        while (currentState != GeneratorState.Stopped)
        {
            
            switch (currentState)
            {
                case GeneratorState.Trail: //jeżeli maszyna stanów jest w stanie Trail (czyli generuje kawałek ogona)
                    {
                        if (currDistance < trailDistance) //jezeli przebyty dystans jest mniejszy od ustalonej długości kawałka to generuj kawałek i zwiększ długość
                        {
                            GenerateTrailPart();
                            currDistance += MovedDistance();
                            SetLastPoints();
                        }
                        else //jeżeli nie wygeneruj tylni kawałek wyzeruj dystans i zmień stan na Gap
                        {
                            GenerateLastPlane();
                            SetLastPoints();
                            currentState = GeneratorState.Gap;
                            currDistance = 0f;
                        }

                        break;
                    }
                case GeneratorState.Gap: //jeżeli maszyna stanów jest w stanie Gap (czyli generuje przerwę)
                    {
                        currDistance += MovedDistance(); //zwiększ przebyty dystans
                        if (currDistance >= gapDistance) //jeżeli przebyty dystans jest większy od ustalonej długości przerwy to 
                        {                              //wygeneruj przedni kawałek,  wyzeruj dystans i zmień stan
                            GenerateFirstPlane();
                            currDistance = 0f;
                            currentState = GeneratorState.Trail;
                        }
                        SetLastPoints();
                        break;
                    }

            }
            yield return new WaitForSeconds(timeToNextUpdate);
        }
    }
    //funkcje zatrzymujące i wznawiające generowanie ścieżki
    public void StopGenerating()
    {
      
        StopCoroutine(trailGenerationCoroutine);
        GenerateLastPlane();
        isGenerating = false;
        this.GetComponent<MeshCollider>().enabled = false;
    }
    public void StartGenerating()
    {
        GenerateFirstPlane();
        trailGenerationCoroutine=StartCoroutine(TrailGenerationDistance());
        isGenerating = true;
        this.GetComponent<MeshCollider>().enabled = true;
    }
    public bool IsGenerating() { return isGenerating; }


    //_DEPRECATED funkcja sprawdzająca czy gracz poruszył się od ostatniego wywołania funkcji GenerateTrailPart
    //przestałem używać bo robi problemy jak gra przylaguje a nie optymalizuje prawie nic
    private bool CheckIfMoved()
    {

        if (Mathf.Abs(Vector3.Distance(headTransforms[0].position, lastPoints[0])) <= distanceTreshold &&
            Mathf.Abs(Vector3.Distance(headTransforms[1].position, lastPoints[1])) <= distanceTreshold &&
            Mathf.Abs(Vector3.Distance(headTransforms[2].position, lastPoints[2])) <= distanceTreshold &&
            Mathf.Abs(Vector3.Distance(headTransforms[3].position, lastPoints[3])) <= distanceTreshold)
            return false;
        else
            return true;
    }

    //zwraca odległość od ostatniego update'a
    private float MovedDistance()
    {
        return (Vector3.Distance(headTransforms[(int)D.UL].position, lastPoints[(int)D.UL])
              + Vector3.Distance(headTransforms[(int)D.UR].position, lastPoints[(int)D.UR])) / 2;
    }
}
