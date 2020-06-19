using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//klasa kontenera na dane przekazywane do powerupa przy inicjalizacji
//utworzona ponieważ szablon wymaga jednego argumentu przy inicjalizacji, a potrzebne są dwa obiekty do działania powerupa
public class DoublePointsPowerupDataContainer
{
    public VRMovement _player { get; set; }
    public Text _text { get; set; }

}

//powerup podwajający czasowo ilość punktów zbieranych przez gracza
public class DoublePointsPowerup : Powerup<DoublePointsPowerupDataContainer>
{
    private VRMovement Player;
    //zmienna przechowująca referencje do obiektu wyświetlającego UI gracza żeby wypisać informacje o podwójnych punktach
    private Text UIText;
    public override void Initialize(DoublePointsPowerupDataContainer c)
    {
        LifeTimeDefault = 10f; 
        LifeTimeCurrent = LifeTimeDefault;
        this.Player = c._player;
        this.UIText = c._text;
        AffectPlayer(false);
        isInitialized = true;
    }
    protected override void AffectPlayer(bool reverse)
    {
        if (Player != null)
        {
            if (!reverse)
            {
                Player.doublePoints = true;
                if (!UIText.text.Contains("Game Over"))
                    UIText.text += "    DP";
            }
            else
            {
                Player.doublePoints = false;
                if(!UIText.text.Contains("Game Over"))
                    UIText.text = UIText.text.Substring(0, UIText.text.Length - 6);
            }
        }
    }
}

