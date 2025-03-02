using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ObjectSpawner : MonoBehaviour
{
    [Tooltip("H�ny target objektumot spawnoljon")]
    public int numberToSpawn = 5;

    [Tooltip("A spawnoland� target objektum")]
    public GameObject target;

    [Tooltip("A quad, aminek a ter�let�n bel�l spawnolunk")]
    public GameObject quad;

    [Tooltip("V�rakoz�si id� a k�vetkez� target spawnol�s�ig")]
    public float spawnDelay = 2.0f;

    [Tooltip("Minim�lis t�vols�g k�t target k�z�tt")]
    public float minDistanceBetweenTargets = 1.0f;

    // Tal�latok ideje �s helyei
    public List<double> hit_times = new List<double>();
    public List<string> hitPlace_fromMiddle = new List<string>();

    // Referencia a PickUpGun komponensre
    public PickUpGun pickUpGun;

    // Priv�t v�ltoz�k a target kezel�shez
    private List<GameObject> activeTargets = new List<GameObject>();
    private int destroyedTargets = 0;

    void Start()
    {
        // Inicializ�ljuk a list�kat
        hit_times = new List<double>();
        hitPlace_fromMiddle = new List<string>();
    }

    void Update()
    {
        // Ellen�rizz�k a megsemmis�tett targeteket
        CheckDestroyedTargets();
    }

    private void CheckDestroyedTargets()
    {
        bool anyDestroyed = false;

        // V�gigmegy�nk a list�n �s elt�vol�tjuk a null elemeket (megsemmis�tett objektumok)
        for (int i = activeTargets.Count - 1; i >= 0; i--)
        {
            if (activeTargets[i] == null)
            {
                activeTargets.RemoveAt(i);
                destroyedTargets++;
                anyDestroyed = true;
            }
        }

        // Ha minden target megsemmis�lt, akkor ledobjuk a fegyvert
        if (anyDestroyed && activeTargets.Count == 0 && destroyedTargets >= numberToSpawn)
        {
            Debug.Log("Minden target megsemmis�lt! �sszesen: " + destroyedTargets);

            if (pickUpGun != null)
            {
                Debug.Log("Fegyver ledob�sa...");
                pickUpGun.DorpWeapon();
            }
        }
    }

    public IEnumerator spawnObject(GameObject targetObject)
    {
        // Ellen�rizz�k, hogy van-e targetObject
        if (targetObject == null)
        {
            Debug.LogError("Nincs megadva target objektum a spawnol�shoz!");
            yield break;
        }

        // Ellen�rizz�k, hogy van-e quad
        if (quad == null)
        {
            Debug.LogError("Nincs megadva quad a spawnol�shoz!");
            yield break;
        }

        // Ellen�rizz�k, hogy a quadnak van-e MeshCollider komponense
        MeshCollider meshCollider = quad.GetComponent<MeshCollider>();
        if (meshCollider == null)
        {
            Debug.LogError("A quadnak nincs MeshCollider komponense!");
            yield break;
        }

        Debug.Log("Target spawnol�s ind�t�sa: " + numberToSpawn + " darab");

        // Resetelj�k a sz�ml�l�kat
        destroyedTargets = 0;
        activeTargets.Clear();

        // Spawnoljuk a megadott sz�m� objektumot
        for (int i = 0; i < numberToSpawn; i++)
        {
            // Keres�nk egy megfelel� poz�ci�t
            Vector3 spawnPosition = FindValidSpawnPosition(meshCollider);

            // Spawnoljuk az objektumot
            GameObject spawnedTarget = Instantiate(targetObject, spawnPosition, quad.transform.rotation);
            activeTargets.Add(spawnedTarget);

            Debug.Log("Target l�trehozva: " + (i + 1) + "/" + numberToSpawn + " poz�ci�: " + spawnPosition);

            // V�letlenszer� v�rakoz�s a k�vetkez� spawnol�sig
            float randomDelay = UnityEngine.Random.Range(spawnDelay * 0.75f, spawnDelay * 1.25f);
            yield return new WaitForSeconds(randomDelay);
        }
    }

    private Vector3 FindValidSpawnPosition(MeshCollider collider)
    {
        float quadX, quadY;
        Vector3 position;
        bool positionValid = false;
        int attempts = 0;
        const int maxAttempts = 30;

        // Alap�rtelmezett �rt�k az unassigned v�ltoz� hib�nak elker�l�s�re
        position = new Vector3(quad.transform.position.x, quad.transform.position.y, quad.transform.position.z);

        while (!positionValid && attempts < maxAttempts)
        {
            // Gener�lunk egy v�letlen poz�ci�t a Quad hat�rain bel�l
            quadX = UnityEngine.Random.Range(collider.bounds.min.x, collider.bounds.max.x);
            quadY = UnityEngine.Random.Range(collider.bounds.min.y, collider.bounds.max.y);
            position = new Vector3(quadX, quadY, quad.transform.position.z);

            // Ellen�rizz�k, hogy el�g t�vol van-e minden m�s akt�v targett�l
            positionValid = IsPositionValid(position);
            attempts++;
        }

        if (attempts >= maxAttempts)
        {
            Debug.LogWarning("Nem siker�lt megfelel� poz�ci�t tal�lni " + maxAttempts + " pr�b�lkoz�s ut�n!");
        }

        return position;
    }

    private bool IsPositionValid(Vector3 position)
    {
        foreach (GameObject obj in activeTargets)
        {
            if (obj != null && Vector3.Distance(position, obj.transform.position) < minDistanceBetweenTargets)
            {
                return false;
            }
        }
        return true;
    }
}