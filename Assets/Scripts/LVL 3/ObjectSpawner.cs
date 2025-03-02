using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ObjectSpawner : MonoBehaviour
{
    [Tooltip("Hány target objektumot spawnoljon")]
    public int numberToSpawn = 5;

    [Tooltip("A spawnolandó target objektum")]
    public GameObject target;

    [Tooltip("A quad, aminek a területén belül spawnolunk")]
    public GameObject quad;

    [Tooltip("Várakozási idõ a következõ target spawnolásáig")]
    public float spawnDelay = 2.0f;

    [Tooltip("Minimális távolság két target között")]
    public float minDistanceBetweenTargets = 1.0f;

    // Találatok ideje és helyei
    public List<double> hit_times = new List<double>();
    public List<string> hitPlace_fromMiddle = new List<string>();

    // Referencia a PickUpGun komponensre
    public PickUpGun pickUpGun;

    // Privát változók a target kezeléshez
    private List<GameObject> activeTargets = new List<GameObject>();
    private int destroyedTargets = 0;

    void Start()
    {
        // Inicializáljuk a listákat
        hit_times = new List<double>();
        hitPlace_fromMiddle = new List<string>();
    }

    void Update()
    {
        // Ellenõrizzük a megsemmisített targeteket
        CheckDestroyedTargets();
    }

    private void CheckDestroyedTargets()
    {
        bool anyDestroyed = false;

        // Végigmegyünk a listán és eltávolítjuk a null elemeket (megsemmisített objektumok)
        for (int i = activeTargets.Count - 1; i >= 0; i--)
        {
            if (activeTargets[i] == null)
            {
                activeTargets.RemoveAt(i);
                destroyedTargets++;
                anyDestroyed = true;
            }
        }

        // Ha minden target megsemmisült, akkor ledobjuk a fegyvert
        if (anyDestroyed && activeTargets.Count == 0 && destroyedTargets >= numberToSpawn)
        {
            Debug.Log("Minden target megsemmisült! Összesen: " + destroyedTargets);

            if (pickUpGun != null)
            {
                Debug.Log("Fegyver ledobása...");
                pickUpGun.DorpWeapon();
            }
        }
    }

    public IEnumerator spawnObject(GameObject targetObject)
    {
        // Ellenõrizzük, hogy van-e targetObject
        if (targetObject == null)
        {
            Debug.LogError("Nincs megadva target objektum a spawnoláshoz!");
            yield break;
        }

        // Ellenõrizzük, hogy van-e quad
        if (quad == null)
        {
            Debug.LogError("Nincs megadva quad a spawnoláshoz!");
            yield break;
        }

        // Ellenõrizzük, hogy a quadnak van-e MeshCollider komponense
        MeshCollider meshCollider = quad.GetComponent<MeshCollider>();
        if (meshCollider == null)
        {
            Debug.LogError("A quadnak nincs MeshCollider komponense!");
            yield break;
        }

        Debug.Log("Target spawnolás indítása: " + numberToSpawn + " darab");

        // Reseteljük a számlálókat
        destroyedTargets = 0;
        activeTargets.Clear();

        // Spawnoljuk a megadott számú objektumot
        for (int i = 0; i < numberToSpawn; i++)
        {
            // Keresünk egy megfelelõ pozíciót
            Vector3 spawnPosition = FindValidSpawnPosition(meshCollider);

            // Spawnoljuk az objektumot
            GameObject spawnedTarget = Instantiate(targetObject, spawnPosition, quad.transform.rotation);
            activeTargets.Add(spawnedTarget);

            Debug.Log("Target létrehozva: " + (i + 1) + "/" + numberToSpawn + " pozíció: " + spawnPosition);

            // Véletlenszerû várakozás a következõ spawnolásig
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

        // Alapértelmezett érték az unassigned változó hibának elkerülésére
        position = new Vector3(quad.transform.position.x, quad.transform.position.y, quad.transform.position.z);

        while (!positionValid && attempts < maxAttempts)
        {
            // Generálunk egy véletlen pozíciót a Quad határain belül
            quadX = UnityEngine.Random.Range(collider.bounds.min.x, collider.bounds.max.x);
            quadY = UnityEngine.Random.Range(collider.bounds.min.y, collider.bounds.max.y);
            position = new Vector3(quadX, quadY, quad.transform.position.z);

            // Ellenõrizzük, hogy elég távol van-e minden más aktív targettõl
            positionValid = IsPositionValid(position);
            attempts++;
        }

        if (attempts >= maxAttempts)
        {
            Debug.LogWarning("Nem sikerült megfelelõ pozíciót találni " + maxAttempts + " próbálkozás után!");
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