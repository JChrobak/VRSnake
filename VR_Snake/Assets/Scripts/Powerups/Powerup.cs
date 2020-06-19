using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//klasa abstarkcyjna dziedziczą po niej wszystkie typy powerupów
//w zależności od typu powerupa mogą być wymagane różne obiekty przy inicjalizacji, dlatego zastosowaliśmy szablon
public abstract class Powerup<TGameObject> : MonoBehaviour
{
    //całkowity czas życia powerupa
    protected float LifeTimeDefault = 5f;
    //aktualny czas życia powerupaa
    [SerializeField] protected float LifeTimeCurrent;
    //flaga mówiąca czy powerup został zainicjalizowany
    protected bool isInitialized = false;

    // Update is called once per frame
    void Update()
    {
        //skróć czas życia powerupa jeżeli został już zainicjalizowany 
        if(isInitialized)
        {
            //jeżeli czas życia dobiegł końca usuń skrypt
            LifeTimeCurrent -= Time.deltaTime;
            if (LifeTimeCurrent <= 0)
                Destroy(this);
        }
    }

    private void OnDestroy()
    {
        AffectPlayer(true);
    }
    //odśwież czas życia powerupa, w niektórych przypadkach (gdy powerup się nie stackuje) jest wywoływana po ponownym zebraniu powerupa przez gracza
    public void Refresh()
    {
        this.LifeTimeCurrent = LifeTimeDefault;
    }

    //abstrakcyjna metoda inicjalizująca powerupa
    public abstract void Initialize(TGameObject obj);
    //abstrakcyjna metoda która wywołuje działanie powerupa
    //w zależności od typu powerupa może on modyfikować same parametry gracza lub jego otoczenia
    //parametr reverse mówi czy efekt ma dopiero wejść w życie czy ma być odwrócony do stanu pierwotnego
    //metoda ta jest wywoływana z parametrem false przed usunięciem skryptu
    protected abstract void AffectPlayer(bool reverse);
}
