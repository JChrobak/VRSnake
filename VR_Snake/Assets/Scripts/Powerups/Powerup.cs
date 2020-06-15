using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Powerup<TGameObject> : MonoBehaviour
{
    //protected float LifeTimeDefault = 5f;
    protected float LifeTimeDefault = 5f;
    [SerializeField] protected float LifeTimeCurrent;
    protected bool isInitialized = false;

    // Update is called once per frame
    void Update()
    {
        if(isInitialized)
        {
            LifeTimeCurrent -= Time.deltaTime;
            if (LifeTimeCurrent <= 0)
                Destroy(this);
        }
    }

    private void OnDestroy()
    {
        AffectPlayer(true);
    }

    public void Refresh()
    {
        this.LifeTimeCurrent = LifeTimeDefault;
    }

    public abstract void Initialize(TGameObject obj);
    protected abstract void AffectPlayer(bool reverse);
}
