using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using Unity.VisualScripting;
using System.Security.Cryptography;
public class ObjectSpawner_1place : MonoBehaviour
{
    public int numberToSpawn = 5; // Hбny objektumot spawnoljunk
    public GameObject trg; // A spawnolandу target objektum
    public GameObject quad; // A Quad, amelynek a kцzepйre spawnoljuk az objektumot
    //public float spawnDelay = 2.0f; // Spawnolбsok kцzцtt eltelt idх (mp-ben)
    public GameObject spawned;
    public bool isSpawned = false;
    public float timer = 0f;
    private Target target_script;
    public List<double> hit_times = new List<double>();
    public List<string> hitPlace_fromMiddle = new List<string>();

    // Szбmlбlу a megsemmisнtett targetek szбmolбsбra
    private int destroyedTargets = 0;

    // Referencia a PickUpGun komponensre
    public PickUpGun pickUpGun;

    void Update()
    {
        //target_script = spawned.GetComponent<Target>();
        if (isSpawned)
        {
            timer += Time.deltaTime;
            destroyedObj(spawned);
        }
    }

    // Objektum spawnolбsa a Quad kцzepйre
    public IEnumerator spawnObject(GameObject obj)
    {
        // Reset the counter at the beginning of a new spawn session
        destroyedTargets = 0;

        float randomNumber = UnityEngine.Random.Range(2f, 5f);
        for (int i = 0; i < numberToSpawn; i++)
        {
            // A Quad kцzйppontjбnak lekйrйse
            Vector3 fit_quad = quad.transform.position;
            // Objektum lйtrehozбsa a Quad kцzйppontjбban
            spawned = Instantiate(obj, fit_quad, quad.transform.rotation);
            isSpawned = true;
            yield return new WaitForSeconds(randomNumber); // Kйsleltetйs a kцvetkezх spawn elхtt
        }
    }

    public void destroyedObj(GameObject obj)
    {
        if (obj.IsDestroyed())
        {
            Debug.Log("Talбlat! Idх spawn йs talбlat kцzцtt: " + Math.Round(timer, 2) + " mp");
            hit_times.Add(Math.Round(timer, 2)); // idх elmentйse
            timer = 0f;
            isSpawned = false;

            // Nцveljьk a megsemmisнtett targetek szбmбt
            destroyedTargets++;

            // Ellenхrizzьk, hogy minden target el lett-e talбlva
            if (destroyedTargets >= numberToSpawn)
            {
                Debug.Log("Minden target eltalбlva! Fegyver ledobбsa...");

                // Ha a pickUpGun referencia lйtezik, hнvjuk meg a DropWeapon() metуdust
                if (pickUpGun != null)
                {
                    pickUpGun.DorpWeapon();
                }
                else
                {
                    Debug.LogError("Nincs beбllнtva a pickUpGun referencia az ObjectSpawner_1place szkriptben!");
                }
            }
        }
    }
}