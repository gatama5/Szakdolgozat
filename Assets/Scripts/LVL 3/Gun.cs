

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

    List<Tuple<double, double>> hitpoints = new List<Tuple<double, double>>(); // találatok
    public float fireRate = 1f;  // A lövésenkénti szünet (másodpercben)
    private float nextFireTime = 0f;
    public float src_volume = 0.5f;
    public ObjectSpawner_1place osp_1place;
    public PickUpGun pck_gun;

    public void Start()
    {
        // Ellenõrizzük és ha null, akkor megpróbáljuk megkeresni
        if (osp_1place == null)
        {
            osp_1place = FindObjectOfType<ObjectSpawner_1place>();
            Debug.Log($"Gun: Found ObjectSpawner_1place: {osp_1place != null}");
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
            if (hit.collider.name == "target_front")
            {
                hitCounter++;
                Vector2 hitPointWorld = hit.point;
                Transform targetFront = hit.transform;
                Vector2 hitPointLocal = targetFront.InverseTransformPoint(hitPointWorld);
                double hitX = Math.Round(hitPointLocal.x * 10, 2);
                double hitY = Math.Round(hitPointLocal.y * 10, 2);

                // Debug log a találat helyérõl
                Debug.Log("Találati pont a target_front közepéhez képest: " + hitX + ", " + hitY);

                // Ellenõrizzük, hogy osp_1place inicializálva van-e
                if (osp_1place != null)
                {
                    // Gyõzõdjünk meg róla, hogy a lista inicializálva van
                    if (osp_1place.hitPlace_fromMiddle == null)
                    {
                        osp_1place.hitPlace_fromMiddle = new List<string>();
                        Debug.Log("Initialized hitPlace_fromMiddle list");
                    }

                    // Formátum változtatás: vesszõ helyett pont használata
                    string formattedPosition = $"{hitX.ToString().Replace(',', '.')},{hitY.ToString().Replace(',', '.')}";
                    osp_1place.hitPlace_fromMiddle.Add($"{hitX}|{hitY}");
                    Debug.Log($"Added position {formattedPosition} to hitPlace_fromMiddle list. New count: {osp_1place.hitPlace_fromMiddle.Count}");

                    // Tároljuk el a saját listánkban is
                    hitpoints.Add(new Tuple<double, double>(hitX, hitY));
                }
                else
                {
                    Debug.LogError("osp_1place is null! Cannot store hit position.");
                }

                Target target = hit.transform.GetComponent<Target>();
                if (target != null)
                {
                    target.TakeDamage(damage);
                }
            }
        }
    }
}
