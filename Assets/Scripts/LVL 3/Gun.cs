using UnityEngine;
using UnityEngine.UIElements;
using System;
using UnityEngine.VFX;
using System.Collections;
using System.Collections.Generic;

public class Gun : MonoBehaviour
{
    public float damage = 1f;
    public float range = 100f;
    public VisualEffect effect;
    public Camera fpsCam;
    public AudioSource src;

    public float fireRate = 1f;  // A l�v�senk�nti sz�net (m�sodpercben)
    private float nextFireTime = 0f;

    public float src_volume = 0.5f;

    public ObjectSpawner_1place osp_1place;

    public void Start()
    {
        osp_1place = new ObjectSpawner_1place();
        src.volume = src_volume;

    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            Shoot();
            effect.Play();
            src.Play();
        }
    }

    

    public void Shoot()
    {
        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range)) 
        {
            if (hit.collider.name == "target_front") //target 
            {
                Vector2 hitPointWorld = hit.point;
                Transform targetFront = hit.transform;  
                Vector2 hitPointLocal = targetFront.InverseTransformPoint(hitPointWorld);
                Debug.Log("Tal�lati pont a target_front k�zep�hez k�pest: " + Math.Round(hitPointLocal.x * 10, 2)  + " " + Math.Round(hitPointLocal.y * 10, 2));
                osp_1place.hitPlace_fromMiddle.Add("X: " + (Math.Round(hitPointLocal.x * 10, 2).ToString() + " Y: " + (Math.Round(hitPointLocal.y * 10, 2)).ToString())); //tal�lati hely elment�se
                Target target = hit.transform.GetComponent<Target>();
                if (target != null)
                {
                    target.TakeDamage(damage);
                }            
            }
        }
    }

}

    
