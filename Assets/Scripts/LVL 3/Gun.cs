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

    List<Tuple<double, double>> hitpoints = new List<Tuple<double, double>>();
    public float fireRate = 1f;
    private float nextFireTime = 0f;
    public float src_volume = 0.5f;
    public ObjectSpawner_1place osp_1place;
    public PickUpGun pck_gun;

    public ObjectSpawner targetObjectSpawner;

    public void Start()
    {
        if (osp_1place == null)
        {
            osp_1place = FindObjectOfType<ObjectSpawner_1place>();
        }

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
            Target target = hit.transform.GetComponent<Target>();
            if (target == null && hit.transform.parent != null)
            {
                target = hit.transform.parent.GetComponent<Target>();
            }

            if (hit.collider.name == "target_front" || target != null)
            {
                hitCounter++;
                Vector2 hitPointWorld = hit.point;
                Transform hitTransform = hit.transform;
                Vector2 hitPointLocal = hitTransform.InverseTransformPoint(hitPointWorld);
                double hitX = Math.Round(hitPointLocal.x * 10, 2);
                double hitY = Math.Round(hitPointLocal.y * 10, 2);

                Debug.Log("Találati pont a target közepéhez képest: " + hitX + ", " + hitY);


                int currentLevel = NextGameColliderScript.GetCurrentLevel();

                if (currentLevel == 1 && targetObjectSpawner != null)
                {
                    if (targetObjectSpawner.hitPlace_fromMiddle == null)
                    {
                        targetObjectSpawner.hitPlace_fromMiddle = new List<string>();
                    }
                    if (targetObjectSpawner.hit_times == null)
                    {
                        targetObjectSpawner.hit_times = new List<double>();
                    }

                    string positionString = $"{hitX}|{hitY}";
                    bool positionExists = false;
                    foreach (string pos in targetObjectSpawner.hitPlace_fromMiddle)
                    {
                        if (pos == positionString)
                        {
                            positionExists = true;
                            break;
                        }
                    }
                    if (!positionExists)
                    {
                        targetObjectSpawner.hitPlace_fromMiddle.Add(positionString);

                    }
                }
                else if (currentLevel == 2 && osp_1place != null)
                {
                    if (osp_1place.hitPlace_fromMiddle == null)
                    {
                        osp_1place.hitPlace_fromMiddle = new List<string>();
                    }

                    string positionString = $"{hitX}|{hitY}";
                    osp_1place.hitPlace_fromMiddle.Add(positionString);

                    hitpoints.Add(new Tuple<double, double>(hitX, hitY));
                }
                if (target != null)
                {
                    target.TakeDamage(damage);
                }
            }
        }
    }

}