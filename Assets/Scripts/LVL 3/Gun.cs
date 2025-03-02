

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

    List<Tuple<double, double>> hitpoints = new List<Tuple<double, double>>(); // tal�latok
    public float fireRate = 1f;  // A l�v�senk�nti sz�net (m�sodpercben)
    private float nextFireTime = 0f;
    public float src_volume = 0.5f;
    public ObjectSpawner_1place osp_1place;
    public PickUpGun pck_gun;

    public void Start()
    {
        // Ellen�rizz�k �s ha null, akkor megpr�b�ljuk megkeresni
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

                // Debug log a tal�lat hely�r�l
                Debug.Log("Tal�lati pont a target_front k�zep�hez k�pest: " + hitX + ", " + hitY);

                // Ellen�rizz�k, hogy osp_1place inicializ�lva van-e
                if (osp_1place != null)
                {
                    // Gy�z�dj�nk meg r�la, hogy a lista inicializ�lva van
                    if (osp_1place.hitPlace_fromMiddle == null)
                    {
                        osp_1place.hitPlace_fromMiddle = new List<string>();
                        Debug.Log("Initialized hitPlace_fromMiddle list");
                    }

                    // Form�tum v�ltoztat�s: vessz� helyett pont haszn�lata
                    string formattedPosition = $"{hitX.ToString().Replace(',', '.')},{hitY.ToString().Replace(',', '.')}";
                    osp_1place.hitPlace_fromMiddle.Add($"{hitX}|{hitY}");
                    Debug.Log($"Added position {formattedPosition} to hitPlace_fromMiddle list. New count: {osp_1place.hitPlace_fromMiddle.Count}");

                    // T�roljuk el a saj�t list�nkban is
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
