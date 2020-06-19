using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//jump powerup - wyrzuca gracza w powietrze
public class JumpPowerup : Powerup<VRMovement>
{
    VRMovement Player;

    public override void Initialize(VRMovement _player)
    {
        LifeTimeDefault = 1.5f; 
        LifeTimeCurrent = LifeTimeDefault;
        this.Player = _player;
        AffectPlayer(false);
        isInitialized = true;

    }
    protected override void AffectPlayer(bool reverse)
    {
        if (Player != null)
        {
            if (!reverse)
            {
                Player.isFlying = true;
            }
            else
            {
                Player.isFlying = false;
            }
        }
    }
}
