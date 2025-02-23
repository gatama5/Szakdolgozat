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

    
     List<Tuple<double, double>> hitpoints = new List<Tuple<double, double>>(); // találatok

    public float fireRate = 1f;  // A lövésenkénti szünet (másodpercben)
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



    //public void Shoot()
    //{
    //    RaycastHit hit;
    //    if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range)) 
    //    {
    //        if (hit.collider.name == "target_front") //target 
    //        {
    //            Vector2 hitPointWorld = hit.point;
    //            Transform targetFront = hit.transform;  
    //            Vector2 hitPointLocal = targetFront.InverseTransformPoint(hitPointWorld);
    //            Debug.Log("Találati pont a target_front közepéhez képest: " + Math.Round(hitPointLocal.x * 10, 2)  + " " + Math.Round(hitPointLocal.y * 10, 2));
    //            osp_1place.hitPlace_fromMiddle.Add("X: " + (Math.Round(hitPointLocal.x * 10, 2).ToString() + " Y: " + (Math.Round(hitPointLocal.y * 10, 2)).ToString())); //találati hely elmentése
    //            hitpoints.Add(new Tuple<double, double>(Math.Round(hitPointLocal.x * 10, 2),Math.Round(hitPointLocal.y * 10, 2)));
    //            Target target = hit.transform.GetComponent<Target>();
    //            if (target != null)
    //            {
    //                target.TakeDamage(damage);
    //            }            
    //        }
    //    }
    //}

    public void Shoot()
    {
        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range))
        {
            if (hit.collider.name == "target_front")
            {
                Vector2 hitPointWorld = hit.point;
                Transform targetFront = hit.transform;
                Vector2 hitPointLocal = targetFront.InverseTransformPoint(hitPointWorld);

                double hitX = Math.Round(hitPointLocal.x * 10, 2);
                double hitY = Math.Round(hitPointLocal.y * 10, 2);

                // Store coordinates in a more database-friendly format
                osp_1place.hitPlace_fromMiddle.Add($"{hitX},{hitY}");
                hitpoints.Add(new Tuple<double, double>(hitX, hitY));

                Target target = hit.transform.GetComponent<Target>();
                if (target != null)
                {
                    target.TakeDamage(damage);
                }
            }
        }
    }

}

    
