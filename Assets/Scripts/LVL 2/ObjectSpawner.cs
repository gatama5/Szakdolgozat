using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ObjectSpawner : MonoBehaviour
{
    [Tooltip("Hбny target objektumot spawnoljon")]
    public int numberToSpawn = 5;

    [Tooltip("A spawnolandу target objektum")]
    public GameObject target;

    [Tooltip("A quad, aminek a terьletйn belьl spawnolunk")]
    public GameObject quad;

    [Tooltip("Vбrakozбsi idх a kцvetkezх target spawnolбsбig")]
    public float spawnDelay = 2.0f;

    [Tooltip("Minimбlis tбvolsбg kйt target kцzцtt")]
    public float minDistanceBetweenTargets = 1.0f;

    // Talбlatok ideje йs helyei
    public List<double> hit_times = new List<double>();
    public List<string> hitPlace_fromMiddle = new List<string>();

    // Referencia a PickUpGun komponensre
    public PickUpGun pickUpGun;

    // Privбt vбltozуk a target kezelйshez
    private List<GameObject> activeTargets = new List<GameObject>();

    // Mуdosнtva private-rуl public-ra, hogy kнvьlrхl is elйrhetх legyen
    public int destroyedTargets = 0;

    private List<float> spawnTimes = new List<float>();

    void Start()
    {
        // Inicializбljuk a listбkat
        hit_times = new List<double>();
        hitPlace_fromMiddle = new List<string>();
    }

    void Update()
    {
        // Ellenхrizzьk a megsemmisнtett targeteket
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
                // Record the hit time (time since spawn) if we can determine it
                if (i < spawnTimes.Count && spawnTimes[i] > 0)
                {
                    double hitTime = Math.Round(Time.time - spawnTimes[i], 2);

                    // Ellenőrizzük, hogy ez a találat már benne van-e a listában
                    bool alreadyExists = false;
                    foreach (double time in hit_times)
                    {
                        if (Math.Abs(time - hitTime) < 0.01) // Kis tolerancia az időbeli eltérésre
                        {
                            alreadyExists = true;
                            Debug.Log($"Duplikált találat kihagyva: {hitTime}");
                            break;
                        }
                    }

                    // Csak akkor mentjük el, ha még nincs a listában
                    if (!alreadyExists)
                    {
                        hit_times.Add(hitTime);
                        Debug.Log("Találat feldolgozva! Idő spawn és találat között: " + hitTime + " mp");

                        // Ez itt a kritikus rész - Ne húzzuk ki kommentbe, tényleg csak akkor mentünk, 
                        // ha nem duplikált a találat és biztosan új
                    }
                }

                activeTargets.RemoveAt(i);
                if (i < spawnTimes.Count)
                    spawnTimes.RemoveAt(i);
                destroyedTargets++;
                anyDestroyed = true;
            }
        }

        // Ha minden target megsemmisült, akkor ledobjuk a fegyvert
        if (anyDestroyed && activeTargets.Count == 0 && destroyedTargets >= numberToSpawn)
        {
            if (pickUpGun != null)
            {
                pickUpGun.DorpWeapon();
            }
        }
    }

    private void SaveHitToDatabase(double hitTime, double posX, double posY)
    {
        SQLiteDBScript dbManager = FindObjectOfType<SQLiteDBScript>();
        if (dbManager != null)
        {
            // Pozíció felülírása, ha szükséges
            if (posX == 0 && posY == 0 && hitPlace_fromMiddle.Count > 0)
            {
                string lastPos = hitPlace_fromMiddle[hitPlace_fromMiddle.Count - 1];
                string[] coordinates = lastPos.Split('|');
                if (coordinates.Length == 2)
                {
                    double.TryParse(coordinates[0], out posX);
                    double.TryParse(coordinates[1], out posY);
                    Debug.Log($"Pozíció felülírva a hitPlace_fromMiddle-ből: ({posX},{posY})");
                }
            }

            // Eldöntjük, hogy ez a találat szerepel-e már az adatbázisban
            // Egyszerűbb megoldás - az UpdateTargetScore metódusban kezeljük a duplikáció ellenőrzést

            // Meghatározzuk a lövés számát
            int shotNumber = hit_times.Count;

            // Mentés az adatbázisba
            dbManager.UpdateTargetScore(shotNumber, hitTime, posX, posY);
            Debug.Log($"Target adat elmentve az adatbázisba: idő={hitTime}, pozíció=({posX},{posY}), sorszám={shotNumber}");
        }
        else
        {
            Debug.LogError("Nem található SQLiteDBScript a jelenetben! Adat nem menthető.");
        }
    }
    public IEnumerator spawnObject(GameObject targetObject)
    {
        // Ellenőrizzük, hogy van-e targetObject
        if (targetObject == null)
        {
            yield break;
        }

        // Ellenőrizzük, hogy van-e quad
        if (quad == null)
        {
            yield break;
        }

        // Ellenőrizzük, hogy a quadnak van-e MeshCollider komponense
        MeshCollider meshCollider = quad.GetComponent<MeshCollider>();
        if (meshCollider == null)
        {
            yield break;
        }

        // Reseteljük a számlálókat
        destroyedTargets = 0;
        activeTargets.Clear();
        spawnTimes.Clear(); // Clear the spawn times list
        hit_times.Clear(); // Clear the hit times

        // Spawnoljuk a megadott számú objektumot
        for (int i = 0; i < numberToSpawn; i++)
        {
            // Keresünk egy megfelelő pozíciót
            Vector3 spawnPosition = FindValidSpawnPosition(meshCollider);

            // Spawnoljuk az objektumot
            GameObject spawnedTarget = Instantiate(targetObject, spawnPosition, quad.transform.rotation);

            // Győződjünk meg róla, hogy a target-nek van Target komponense
            Target targetComponent = spawnedTarget.GetComponentInChildren<Target>();
            if (targetComponent == null)
            {
                targetComponent = spawnedTarget.AddComponent<Target>();
            }

            activeTargets.Add(spawnedTarget);
            spawnTimes.Add(Time.time); // Record the spawn time

            // Véletlenszerű várakozás a következő spawnolásig
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

        // Alapйrtelmezett йrtйk az unassigned vбltozу hibбnak elkerьlйsйre
        position = new Vector3(quad.transform.position.x, quad.transform.position.y, quad.transform.position.z);

        while (!positionValid && attempts < maxAttempts)
        {
            // Generбlunk egy vйletlen pozнciуt a Quad hatбrain belьl
            quadX = UnityEngine.Random.Range(collider.bounds.min.x, collider.bounds.max.x);
            quadY = UnityEngine.Random.Range(collider.bounds.min.y, collider.bounds.max.y);
            position = new Vector3(quadX, quadY, quad.transform.position.z);

            // Ellenхrizzьk, hogy elйg tбvol van-e minden mбs aktнv targettхl
            positionValid = IsPositionValid(position);
            attempts++;
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