using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManagerScript : MonoBehaviour
{
    //powrót do menu przy dotknięciu ekranu
    void Update()
    {
        if (Input.touchCount > 0)
            SceneManager.LoadScene("MainMenu");
    }
}