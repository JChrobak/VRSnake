using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

//funkcja obsługująca poruszanie się i kolizje gracza
public class VRMovement : MonoBehaviour
{
    public float mSpeed = 3.0f; //prędkość poruszania
    public float gravity = 2.0f; //siła gravitacji
    private bool isMoving = true; //zmienna przechowująca informacje czy gracz się posrusza

    private CharacterController characterController;
    [SerializeField] private Transform cameraTransform; //pozycja i rotacja kamery
    [SerializeField] private Transform snakeHead; //pozycja i rotacja głowy węża
    [SerializeField] private GameObject trailGenerator; //obiekt generujący ogon węża
    [SerializeField] private Material deadMaterial; //materiał do klona zostawionego po śmierci
    [SerializeField] private GameObject uiCanvas; //obiekt wyświetlający ui

    public bool doublePoints = false; //flaga oznaczająca podwójne punkty
    public bool isFlying = false; //flaga oznaczająca latanie

    public int points;

    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        points = 0;
        string str = uiCanvas.GetComponent<Text>().text;
        uiCanvas.GetComponent<Text>().text = "Points: " + points.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        if (isMoving)
        {
            //ustaw rotację w osi y głowy węża na rotację kamery
            snakeHead.rotation = Quaternion.Euler(0, cameraTransform.localEulerAngles.y, 0);
            //oblicz wektor do przodu  na podstawie głowy węża
            Vector3 vForward = snakeHead.transform.forward;
            
            if (!isFlying)
            {
                //zastosuj siłę grawitacji
                vForward.y = -gravity;
            }
                
            else
            {
                //powerup latanie
                vForward.y = gravity * mSpeed * 0.5f;
            }
            //przesuń gracza o wypadkowy wektor * prędkość
            vForward.x *= mSpeed;
            vForward.z *= mSpeed;
            characterController.Move(vForward * Time.deltaTime);
        }

    }
    //detekcja kolizji z lavą
    public void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.tag == "Lava")
            CollisionHandler(hit.gameObject);
    }
    //funkcja zanjmująca się obslugą kolizji, publiczna bo może być wywołana z przedniego collidera
    public void CollisionHandler(GameObject other) {
        
        if ((trailGenerator.GetComponent<TrailGenerator>()!=null && trailGenerator.GetComponent<TrailGenerator>().IsGenerating())||(trailGenerator.GetComponent<SnakeTrailGenerator>()!=null && trailGenerator.GetComponent<SnakeTrailGenerator>().IsGenerating()))
        {
            Debug.Log("Detected collision with [" + other.tag + "]");
            //kolizja z powerupem
            if (other.tag.StartsWith("Powerup"))
            {
                string type = other.tag.Replace("Powerup","");
                Destroy(other);
                GameObject parent = this.transform?.parent.gameObject;
                AttachPowerup(type,parent);
            }
            //kolizja z punktem
            else if (other.tag == "Point")
            {
                Destroy(other);
                if (doublePoints)
                {
                    points += 2;
                    trailGenerator.GetComponent<SnakeTrailGenerator>()?.LenghtenTrail();
                }
                else
                    points++;
                //wydłużenie długości ogona jeżeli jesteśmy w trybie snake
                trailGenerator.GetComponent<SnakeTrailGenerator>()?.LenghtenTrail();
                //nadpisanie ilości punktów
                string str = uiCanvas.GetComponent<Text>().text;
                Regex regex = new Regex("[0-9]+");
                str=regex.Replace(str, points.ToString(), 1);
                uiCanvas.GetComponent<Text>().text = str;
            }
            //kolizja z innym elementem
            else
                KillPlayer();
        }
    }

    //funkcja dodająca skrypt z powerupem do obkietu gracza
    private void AttachPowerup(string type, GameObject player)
    {
        if (player != null)
        {
            //w zależności od typu powerupa dodaj odpowiedni skrypt
            switch (type)
            {
                case "Speed":
                    {
                        SpeedPowerup powerup = player.AddComponent<SpeedPowerup>();
                        powerup?.Initialize(this);
                        break;
                    }
                case "DoublePoints":
                    {
                        DoublePointsPowerup powerup = player.GetComponent<DoublePointsPowerup>();
                        if (powerup != null)
                            powerup.Refresh();
                        else
                        {
                            powerup = player.AddComponent<DoublePointsPowerup>();
                            //powerup double points wymaga kontenera na dane wejściowe przy utworzeniu
                            powerup?.Initialize(new DoublePointsPowerupDataContainer
                            {
                                _player = this,
                                _text = uiCanvas.GetComponent<Text>()
                            });
                        }
                        break;
                    }
                case "Jump":
                    {
                        JumpPowerup powerup = player.GetComponent<JumpPowerup>();
                        if (powerup != null)
                            powerup.Refresh();
                        else
                        {
                            powerup = player.AddComponent<JumpPowerup>();
                            powerup?.Initialize(this);
                        }
                        break;
                    }
                    
                default:
                    Debug.Log("Unknown powerup type: " + type);
                    break;
            }
        }
        else
            Debug.Log("Error getting parent when attaching powerup");
    }
    //funkcja zabijająca gracza, w chwili obecnej na potrzeby testów zostawia klona głowy w miejscu śmierci żeby można było zweryfikować kolizję
    private void KillPlayer()
    {
        trailGenerator.GetComponent<TrailGenerator>()?.StopGenerating();
        trailGenerator.GetComponent<SnakeTrailGenerator>()?.StopGenerating();
        GameObject playerHead = this.transform.Find("HeadObject").gameObject;
        GameObject temp = Instantiate(playerHead, playerHead.transform.position, playerHead.transform.rotation);
        temp.layer = 0;
        temp.GetComponent<MeshRenderer>().material = deadMaterial;
        uiCanvas.GetComponent<Text>().text = "Game Over!\nFinal score: " + points.ToString();
        Handheld.Vibrate();
    }


}
