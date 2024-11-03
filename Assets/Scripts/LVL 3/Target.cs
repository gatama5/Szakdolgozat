using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class Target : MonoBehaviour
{
    public float healt = 1f;
    public bool isTargetHit = false;

    public void TakeDamage(float amount)
    {
        healt -= amount;
        isTargetHit = true;
        if (healt < 1)
        {
            Die();
            isTargetHit = false;
        }
    }

    public void Die()
    {
        Destroy(gameObject);
        Destroy(transform.parent.gameObject);
    }

    public bool HittedObject() 
    {
        return isTargetHit;
    }

}
