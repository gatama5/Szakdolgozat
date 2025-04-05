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

        if (healt <= 0)
        {
            Die();
            isTargetHit = false;
        }
    }

    public void Die()
    {

        if (transform.parent != null)
        {
            Destroy(transform.parent.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool HittedObject()
    {
        return isTargetHit;
    }
}