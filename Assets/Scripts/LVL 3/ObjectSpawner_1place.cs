using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using Unity.VisualScripting;
using System.Security.Cryptography;
public class ObjectSpawner_1place : MonoBehaviour
{
    public int numberToSpawn = 5; // Hány objektumot spawnoljunk
    public GameObject trg; // A spawnolandó target objektum
    public GameObject quad; // A Quad, amelynek a közepére spawnoljuk az objektumot
    //public float spawnDelay = 2.0f; // Spawnolások között eltelt idõ (mp-ben)
    public GameObject spawned;
    public bool isSpawned = false;
    public float timer = 0f;
    private Target target_script;
    public List<double> hit_times = new List<double>();
    public List<string> hitPlace_fromMiddle = new List<string>();

    // Számláló a megsemmisített targetek számolására
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

    // Objektum spawnolása a Quad közepére
    public IEnumerator spawnObject(GameObject obj)
    {
        // Reset the counter at the beginning of a new spawn session
        destroyedTargets = 0;

        float randomNumber = UnityEngine.Random.Range(2f, 5f);
        for (int i = 0; i < numberToSpawn; i++)
        {
            // A Quad középpontjának lekérése
            Vector3 fit_quad = quad.transform.position;
            // Objektum létrehozása a Quad középpontjában
            spawned = Instantiate(obj, fit_quad, quad.transform.rotation);
            isSpawned = true;
            yield return new WaitForSeconds(randomNumber); // Késleltetés a következõ spawn elõtt
        }
    }

    public void destroyedObj(GameObject obj)
    {
        if (obj.IsDestroyed())
        {
            Debug.Log("Találat! Idõ spawn és találat között: " + Math.Round(timer, 2) + " mp");
            hit_times.Add(Math.Round(timer, 2)); // idõ elmentése
            timer = 0f;
            isSpawned = false;

            // Növeljük a megsemmisített targetek számát
            destroyedTargets++;

            // Ellenõrizzük, hogy minden target el lett-e találva
            if (destroyedTargets >= numberToSpawn)
            {
                Debug.Log("Minden target eltalálva! Fegyver ledobása...");

                // Ha a pickUpGun referencia létezik, hívjuk meg a DropWeapon() metódust
                if (pickUpGun != null)
                {
                    pickUpGun.DorpWeapon();
                }
                else
                {
                    Debug.LogError("Nincs beállítva a pickUpGun referencia az ObjectSpawner_1place szkriptben!");
                }
            }
        }
    }
}