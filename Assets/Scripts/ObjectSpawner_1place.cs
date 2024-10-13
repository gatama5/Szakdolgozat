using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using Unity.VisualScripting;

public class ObjectSpawner_1place : MonoBehaviour
{

    public int numberToSpawn = 5; // Hány objektumot spawnoljunk
    public GameObject trg; // A spawnolandó target objektum
    public GameObject quad; // A Quad, amelynek a közepére spawnoljuk az objektumot
    public float spawnDelay = 2.0f; // Spawnolások között eltelt idõ (mp-ben)

    public GameObject spawned;
    public bool isSpawned = false;
    public float timer = 0f;
    private Target target_script;


    void Start()
    {

        // Aszinkron késleltetett spawn ráta
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

    // Objektum spawnolása a Quad közepére
    public IEnumerator spawnObject(GameObject obj)
    {
        for (int i = 0; i < numberToSpawn; i++)
        {
            // A Quad középpontjának lekérése
            Vector3 fit_quad = quad.transform.position;

            // Objektum létrehozása a Quad középpontjában
            spawned = Instantiate(obj, fit_quad, quad.transform.rotation);
            isSpawned = true;
            
            yield return new WaitForSeconds(spawnDelay); // Késleltetés a következõ spawn elõtt
        }
    }

    public void destroyedObj(GameObject obj)
    {
        if (obj.IsDestroyed())
        {
            Debug.Log("Találat! Idõ spawn és találat között: " + Math.Round(timer, 2) + " mp");
            timer = 0f;
            isSpawned = false;
        }
    }
}

