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

        // Hнvjuk a Die() metуdust, ha az йleterх 0 vagy kevesebb
        if (healt <= 0)
        {
            Die();
            isTargetHit = false;
        }
    }

    public void Die()
    {

        // Ha van szьlх objektum, azt is megsemmisнtjьk
        if (transform.parent != null)
        {
            Destroy(transform.parent.gameObject);
        }
        else
        {
            // Ha nincs szьlх, csak ezt az objektumot semmisнtjьk meg
            Destroy(gameObject);
        }
    }

    public bool HittedObject()
    {
        return isTargetHit;
    }
}