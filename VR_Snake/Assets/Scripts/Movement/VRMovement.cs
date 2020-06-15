﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VRMovement : MonoBehaviour
{
    public float mSpeed = 3.0f; //prędkość poruszania
    public float gravity = 2.0f; //siła gravitacji
    private bool isMoving = true; //zmienna przechowująca informacje czy gracz się posrusza

    private CharacterController characterController;
    [SerializeField] private Transform cameraTransform; //pozycja i rotacja kamery
    [SerializeField] private Transform snakeHead; //pozycja i rotacja głowy węża
    [SerializeField] private GameObject trailGenerator; //obiekt generujący ogon węża
    [SerializeField] private Material deadMaterial;
    [SerializeField] private GameObject uiCanvas;

    public bool doublePoints = false;
    public bool isFlying = false;

    public int points;

    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        points = 0;
        uiCanvas.GetComponent<Text>().text="Points: "+points.ToString();
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
                vForward.y = gravity * 2.0f;
            }
            //przesuń gracza o wypadkowy wektor * prędkość
            characterController.Move(vForward*mSpeed * Time.deltaTime);
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
            if (other.tag.StartsWith("Powerup"))
            {
                string type = other.tag.Replace("Powerup","");
                Destroy(other);
                GameObject parent = this.transform?.parent.gameObject;
                AttachPowerup(type,parent);
            }
            else if (other.tag == "Point")
            {
                Destroy(other);
                if (doublePoints)
                    points += 2;
                else
                    points++;
                trailGenerator.GetComponent<SnakeTrailGenerator>()?.LenghtenTrail();
                uiCanvas.GetComponent<Text>().text = "Points: " + points.ToString();
            }
            else
                KillPlayer();
        }
    }

    private void AttachPowerup(string type, GameObject player)
    {
        if (player != null)
        {
            switch (type)
            {
                case "Speed":
                    SpeedPowerup powerup = player.AddComponent<SpeedPowerup>();
                    powerup?.Initialize(this);
                    break;
                case "DoublePoints":
                    DoublePointsPowerup powerup1 = player.AddComponent<DoublePointsPowerup>();
                    powerup1?.Initialize(this);
                    break;
                case "Jump":
                    JumpPowerup powerup2 = player.AddComponent<JumpPowerup>();
                    powerup2?.Initialize(this);
                    break;
                default:
                    Debug.Log("Unknown powerup type: " + type);
                    break;
            }
        }
        else
            Debug.Log("Error getting parent when attaching powerup");
    }
    //funkcja zabijająca gracza, w chwili obecnej na potrzeby testów zostawia klona głowy w miejscu śmierci
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
