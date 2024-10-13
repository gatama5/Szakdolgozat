using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using Unity.VisualScripting;

public class ObjectSpawner_1place : MonoBehaviour
{

    public int numberToSpawn = 5; // H�ny objektumot spawnoljunk
    public GameObject trg; // A spawnoland� target objektum
    public GameObject quad; // A Quad, amelynek a k�zep�re spawnoljuk az objektumot
    public float spawnDelay = 2.0f; // Spawnol�sok k�z�tt eltelt id� (mp-ben)

    public GameObject spawned;
    public bool isSpawned = false;
    public float timer = 0f;
    private Target target_script;


    void Start()
    {

        // Aszinkron k�sleltetett spawn r�ta
        StartCoroutine(spawnObject(trg));
    }


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
        for (int i = 0; i < numberToSpawn; i++)
        {
            // A Quad k�z�ppontj�nak lek�r�se
            Vector3 fit_quad = quad.transform.position;

            // Objektum l�trehoz�sa a Quad k�z�ppontj�ban
            spawned = Instantiate(obj, fit_quad, quad.transform.rotation);
            isSpawned = true;
            
            yield return new WaitForSeconds(spawnDelay); // K�sleltet�s a k�vetkez� spawn el�tt
        }
    }

    public void destroyedObj(GameObject obj)
    {
        if (obj.IsDestroyed())
        {
            Debug.Log("Tal�lat! Id� spawn �s tal�lat k�z�tt: " + Math.Round(timer, 2) + " mp");
            timer = 0f;
            isSpawned = false;
        }
    }
}

