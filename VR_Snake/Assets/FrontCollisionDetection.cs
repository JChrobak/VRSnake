﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrontCollisionDetection : MonoBehaviour
{

    private void OnCollisionEnter(Collision collision)
    {
        //sprawdź czy wywołuje to faktyczna głowa czy klon tworzony po śmierci (klon to sama kostka i jej obiekty dzieci więc jeżeli rodzic tego rodzica jest null to znaczy że jesteśmy w klonie)
        if (this.transform.parent.parent != null)
        {
            Debug.Log("Collision from front collider");
            this.transform.parent.transform.parent.GetComponent<VRMovement>().CollisionHandler(collision.gameObject);
        }
        //usuń skrypt z klona
        else
            Destroy(this.GetComponent<FrontCollisionDetection>());
    }
    
    
}
