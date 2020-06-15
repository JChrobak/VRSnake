using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPowerup : Powerup<VRMovement>
{
    VRMovement Player;

    public override void Initialize(VRMovement _player)
    {
        LifeTimeCurrent = LifeTimeDefault;
        this.Player = _player;
        AffectPlayer(false);
        isInitialized = true;
        LifeTimeDefault = 2f;
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
