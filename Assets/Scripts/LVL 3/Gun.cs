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
    public int hitCounter = 0;

    List<Tuple<double, double>> hitpoints = new List<Tuple<double, double>>(); // talбlatok
    public float fireRate = 1f;  // A lцvйsenkйnti szьnet (mбsodpercben)
    private float nextFireTime = 0f;
    public float src_volume = 0.5f;
    public ObjectSpawner_1place osp_1place;
    public PickUpGun pck_gun;

    public ObjectSpawner targetObjectSpawner; // Ъj vбltozу az ObjectSpawner referenciбhoz

    public void Start()
    {
        // Ellenхrizzьk йs ha null, akkor megprуbбljuk megkeresni
        if (osp_1place == null)
        {
            osp_1place = FindObjectOfType<ObjectSpawner_1place>();
        }

        // Ъj kуd: ObjectSpawner keresйse
        if (targetObjectSpawner == null)
        {
            targetObjectSpawner = FindObjectOfType<ObjectSpawner>();
        }

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
            // Mуdosнtott feltйtel - ellenхrzйs javнtva, hogy mыkцdjцn bбrmilyen target objektummal
            Target target = hit.transform.GetComponent<Target>();
            if (target == null && hit.transform.parent != null)
            {
                target = hit.transform.parent.GetComponent<Target>();
            }

            // Minden target-front elemre vagy bбrmely Target komponenssel rendelkezх elemre mыkцdjцn
            if (hit.collider.name == "target_front" || target != null)
            {
                hitCounter++;
                Vector2 hitPointWorld = hit.point;
                Transform hitTransform = hit.transform;
                Vector2 hitPointLocal = hitTransform.InverseTransformPoint(hitPointWorld);
                double hitX = Math.Round(hitPointLocal.x * 10, 2);
                double hitY = Math.Round(hitPointLocal.y * 10, 2);

                // Debug log a talбlat helyйrхl
                Debug.Log("Talбlati pont a target kцzйpйhez kйpest: " + hitX + ", " + hitY);

                // Ellenхrizzьk, hogy osp_1place inicializбlva van-e
                if (osp_1place != null)
                {
                    // Gyхzхdjьnk meg rуla, hogy a lista inicializбlva van
                    if (osp_1place.hitPlace_fromMiddle == null)
                    {
                        osp_1place.hitPlace_fromMiddle = new List<string>();
                        Debug.Log("Initialized hitPlace_fromMiddle list");
                    }

                    // Formбtum vбltoztatбs: vesszх helyett pont hasznбlata
                    string formattedPosition = $"{hitX.ToString().Replace(',', '.')},{hitY.ToString().Replace(',', '.')}";
                    osp_1place.hitPlace_fromMiddle.Add($"{hitX}|{hitY}");

                    // Tбroljuk el a sajбt listбnkban is
                    hitpoints.Add(new Tuple<double, double>(hitX, hitY));
                }

                // ЪJ KУD: ObjectSpawner frissнtйse
                if (targetObjectSpawner != null)
                {
                    // Gyхzхdjьnk meg rуla, hogy a lista inicializбlva van
                    if (targetObjectSpawner.hitPlace_fromMiddle == null)
                    {
                        targetObjectSpawner.hitPlace_fromMiddle = new List<string>();
                    }

                    if (targetObjectSpawner.hit_times == null)
                    {
                        targetObjectSpawner.hit_times = new List<double>();
                    }

                    // Talбlat idejйnek йs pozнciуjбnak eltбrolбsa
                    targetObjectSpawner.hitPlace_fromMiddle.Add($"{hitX}|{hitY}");
                    double currentTime = Math.Round(Time.time - nextFireTime + fireRate, 2); // Reakciуidх szбmнtбsa
                    targetObjectSpawner.hit_times.Add(currentTime);

                }

                // Mindig keressьnk Target komponenst йs ellenхrizzьk, hogy mйg nem null-e
                if (target != null)
                {
                    target.TakeDamage(damage);
                }
            }
        }
    }
}