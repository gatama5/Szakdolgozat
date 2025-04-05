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

                // Debug log a találat helyéről
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

                        // Ne mentsük itt az időt, azt az ObjectSpawner saját logikája fogja kezelni
                        // Így elkerüljük a duplikációt
                    }
                }
                // 2-es szint = Shooting játék (ObjectSpawner_1place)
                else if (currentLevel == 2 && osp_1place != null)
                {
                    // Ellenőrizzük, hogy a lista inicializálva van-e
                    if (osp_1place.hitPlace_fromMiddle == null)
                    {
                        osp_1place.hitPlace_fromMiddle = new List<string>();
                    }

                    // Formátum változtatás: vessző helyett pont használata
                    string positionString = $"{hitX}|{hitY}";
                    osp_1place.hitPlace_fromMiddle.Add(positionString);

                    // Tároljuk el a saját listánkban is
                    hitpoints.Add(new Tuple<double, double>(hitX, hitY));
                }

                // Mindig keressünk Target komponenst és ellenőrizzük, hogy még nem null-e
                if (target != null)
                {
                    target.TakeDamage(damage);
                }
            }
        }
    }

}