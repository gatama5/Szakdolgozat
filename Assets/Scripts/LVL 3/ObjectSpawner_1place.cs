using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using Unity.VisualScripting;
using System.Security.Cryptography;
public class ObjectSpawner_1place : MonoBehaviour
{
    public int numberToSpawn = 5; // H�ny objektumot spawnoljunk
    public GameObject trg; // A spawnoland� target objektum
    public GameObject quad; // A Quad, amelynek a k�zep�re spawnoljuk az objektumot
    //public float spawnDelay = 2.0f; // Spawnol�sok k�z�tt eltelt id� (mp-ben)
    public GameObject spawned;
    public bool isSpawned = false;
    public float timer = 0f;
    private Target target_script;
    public List<double> hit_times = new List<double>();
    public List<string> hitPlace_fromMiddle = new List<string>();

    // Sz�ml�l� a megsemmis�tett targetek sz�mol�s�ra
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

    // Objektum spawnol�sa a Quad k�zep�re
    public IEnumerator spawnObject(GameObject obj)
    {
        // Reset the counter at the beginning of a new spawn session
        destroyedTargets = 0;

        float randomNumber = UnityEngine.Random.Range(2f, 5f);
        for (int i = 0; i < numberToSpawn; i++)
        {
            // A Quad k�z�ppontj�nak lek�r�se
            Vector3 fit_quad = quad.transform.position;
            // Objektum l�trehoz�sa a Quad k�z�ppontj�ban
            spawned = Instantiate(obj, fit_quad, quad.transform.rotation);
            isSpawned = true;
            yield return new WaitForSeconds(randomNumber); // K�sleltet�s a k�vetkez� spawn el�tt
        }
    }

    public void destroyedObj(GameObject obj)
    {
        if (obj.IsDestroyed())
        {
            Debug.Log("Tal�lat! Id� spawn �s tal�lat k�z�tt: " + Math.Round(timer, 2) + " mp");
            hit_times.Add(Math.Round(timer, 2)); // id� elment�se
            timer = 0f;
            isSpawned = false;

            // N�velj�k a megsemmis�tett targetek sz�m�t
            destroyedTargets++;

            // Ellen�rizz�k, hogy minden target el lett-e tal�lva
            if (destroyedTargets >= numberToSpawn)
            {
                Debug.Log("Minden target eltal�lva! Fegyver ledob�sa...");

                // Ha a pickUpGun referencia l�tezik, h�vjuk meg a DropWeapon() met�dust
                if (pickUpGun != null)
                {
                    pickUpGun.DorpWeapon();
                }
                else
                {
                    Debug.LogError("Nincs be�ll�tva a pickUpGun referencia az ObjectSpawner_1place szkriptben!");
                }
            }
        }
    }
}