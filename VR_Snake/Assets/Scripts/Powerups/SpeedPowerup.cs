using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//speed powerup - przyspiesza gracza
public class SpeedPowerup : Powerup<VRMovement>
{
    VRMovement Player;
    public override void Initialize(VRMovement _player)
    {
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
                Player.mSpeed *= 2;
            }
            else
            {
                Player.mSpeed /= 2;
            }
        }
    }
}
