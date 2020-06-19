using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//enum utworzony żeby ułatwić odwoływanie się do tablic markerów znajduje się w pliku TrailGenerator.cs
//poszczególne wartości odwołują się do poszczególnych markerów
//UL - upper left
//UR - upper right
//BL - bottom left
//BR - bottom right

//aby odwołać się trzeba zrzutować enum na inta 
//np. tablica[(int)d.UL]

//dodałem na wszelki wyapdek jakby coś się stalo z plikiem TrailGenerator.cs
//enum D { UL = 0, UR = 1, BL = 2, BR = 3 }
//enum GeneratorState { Stopped = 0, Trail = 1, Gap = 2, Replacing = 3 }

//klasa generująca ślad gracza na zasadach podobnych jak w grze Snake
//ślad przesuwa się za graczem a jego maksymalna długość jest wydłużana wraz ze zbieranymi punktami
//generowany ślad jest obiektem typu mesh
//korzystamy z korutyn dla zwiększenia możliwości zarządzania czasem podczas uruchomienia generatora
public class SnakeTrailGenerator : MonoBehaviour
{
    private float currDistance;
    private GeneratorState currentState; //maszyna stanów mówiąca w jakim stanie znajduje sieaktualnie generator
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
    [SerializeField] private float defaultLenghtenAmmount = 5f;//domyślna wartość o którą powiększyć ogon
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
        trailGenerationCoroutine = StartCoroutine(TrailGenerationDistance());
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

    // Update is called once per frame
    void Update()
    { }

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

    //funkcja która zastępuje segment czyli usuwa ostatni fragment ogona i generuje na przedostatnim tylną ściankę
    void ReplaceLastSegment()
    {
        currDistance -= RemovedDistance();
        //usuwam pierwsze 30 punktów z wektora vertices (6 punktów na pierwszą ściankę, 24 na boczne, górną i dolną ściankę)
        vertices.RemoveRange(0, 30);
        List<Vector3> temp = new List<Vector3>();
        //CZYTAJ TO BO TO CO TU ROBIE NIE JEST TAKIE OCZYWISTE
        //teraz w wektorze jest kolejny kawałek ogona do którego muszę dogenerować tylną ściankę
        //funkcja GenerateLastPlane nie zadziała ponieważ on generuje ostatnią ściankę na podstawie aktualnych wartości tablicy lastPoints, a ta już jest dużo dalej
        //dlatego muszę dostać się do wartości tablicy lastPoints w momencie generowania kawałka ogona który aktualnie jest ostatni
        //ponieważ funkcja generująca nowe kawałki zawsze dodaje odpowienie punkty w tej samej kolejności możemy wynioskować jakie punkty aktualnie pozostały w tablicy
        //punkty dodawane są w następującej kolejności 
        //HUL, LUL, LBL, HUL, LBL, HBL, HBL, LBR, LUR, HUR...
        //(legenda - pierwsza litera (H lub L) oznacza czy punkt pochodzi z tablicy headTrnsforms czy lastPoints, pozostałe litert to pozycja pubktu tak jak w enumie (UL to upper left czyli lewy górny, BR to bottom right itd.) 
        //nas interesuje tablica lastPoints w tamtym momencie czyli skupimy się na punktach z pierwszą literą L
        //aby utworzyć tylną ściankę należy podać punkty w następującej kolejności
        //(L)UL, (L)UR, (L)BL, (L)UR, (L)BR, (L)BL
        //czyli będą to punkty które aktualnie znajdują się w liście na pozycjach
        //   1      7      2      7      6      2
        //i dokładnie w takiej kolejności je dodaje do nowej tablicy
        temp.Add(vertices[1]);
        temp.Add(vertices[7]);
        temp.Add(vertices[2]);
        temp.Add(vertices[7]);
        temp.Add(vertices[6]);
        temp.Add(vertices[2]);
        //dodaje na koniec temp starą listę i przypisuje zmienną temp do zmiennej vertices
        temp.AddRange(vertices);
        vertices = temp;
        //usuwam ostatnie 24 elementów z listy trójkąty i zmniejszam zmienną currTriangleNo o 24 aby wziąć pod uwagę usunięty segment
        triangles.RemoveRange(triangles.Count - 24, 24);
        currTriangleNo -= 24;
    }
    //główna korutyna generująca ogon gracza
    IEnumerator TrailGenerationDistance()
    {
        //generowanie pierwszego kawałka przedniego
        GenerateFirstPlane();
        SetLastPoints();
        yield return new WaitForSeconds(timeToNextUpdate);
        while (currentState != GeneratorState.Stopped)
        {

            switch (currentState)
            {
                case GeneratorState.Trail: //jeżeli maszyna stanów jest w stanie Trail (czyli generuje kawałek ogona)
                    {
                        GenerateTrailPart();
                        currDistance += MovedDistance();
                        SetLastPoints();
                        if (currDistance >= trailDistance)
                            currentState = GeneratorState.Replacing;
                        break;
                    }
                case GeneratorState.Replacing: //jeżeli maszyna stanów jest w stanie Replacing (czyli zastępuje ostatnie kawałki ogona nowymi)
                    {
                        while (currDistance >= trailDistance)
                            ReplaceLastSegment();
                        GenerateTrailPart();
                        currDistance += MovedDistance();
                        SetLastPoints();
                        if (currDistance < trailDistance)
                            currentState = GeneratorState.Trail;
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
    public bool IsGenerating() { return isGenerating; }

    //zwraca odległość od ostatniego update'a
    private float MovedDistance()
    {
        return (Vector3.Distance(headTransforms[(int)D.UL].position, lastPoints[(int)D.UL])
              + Vector3.Distance(headTransforms[(int)D.UR].position, lastPoints[(int)D.UR])) / 2;
    }

    //zwraca odległość usuniętą przy usunięciu ostatniego fragmentu ogona
    //WYWOŁYWAĆ PRZED USUNIĘCIEM TYLNEJ ŚCIANKI
    private float RemovedDistance()
    {
        //funkcja ta jest wywoływana w momencie gdy istnieje tylna ścianka dlatego:
        //punkt o indeksie 0 to UL z tablicy lastPoints (w momencie tworzenia kawałka)
        //punkt o indeksie 6 to UL z tablicy headTransforms (w momencie tworzenia kawałka)
        //punkt o indeksie 1 to UR z tablicy lastPoints (w momencie tworzenia kawałka)
        //punkt o indeksie 14 to UR z tablicy headTransforms (w momencie tworzenia kawałka)
        return (Vector3.Distance(vertices[0], vertices[6]) + Vector3.Distance(vertices[1], vertices[14])) / 2;
    }

    //funkcja wydłużająca ogon o podaną wartość
    public void LenghtenTrail(float ammount = -1)
    {
        if (ammount > 0)
            trailDistance += ammount;
        else
            trailDistance += defaultLenghtenAmmount;
    }

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
}
