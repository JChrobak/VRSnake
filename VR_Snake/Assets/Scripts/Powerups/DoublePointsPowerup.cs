using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DoublePointsPowerupDataContainer
{
    public VRMovement _player { get; set; }
    public Text _text { get; set; }

}

public class DoublePointsPowerup : Powerup<DoublePointsPowerupDataContainer>
{
    [SerializeField]private VRMovement Player;
    [SerializeField]private Text UIText;
    public override void Initialize(DoublePointsPowerupDataContainer c)
    {
        LifeTimeCurrent = LifeTimeDefault;
        this.Player = c._player;
        this.UIText = c._text;
        AffectPlayer(false);
        isInitialized = true;
        LifeTimeDefault = 10f;
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

