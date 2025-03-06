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

        // Vйgigmegyьnk a listбn йs eltбvolнtjuk a null elemeket (megsemmisнtett objektumok)
        for (int i = activeTargets.Count - 1; i >= 0; i--)
        {
            if (activeTargets[i] == null)
            {
                activeTargets.RemoveAt(i);
                destroyedTargets++;
                anyDestroyed = true;
            }
        }

        // Ha minden target megsemmisьlt, akkor ledobjuk a fegyvert
        if (anyDestroyed && activeTargets.Count == 0 && destroyedTargets >= numberToSpawn)
        {

            if (pickUpGun != null)
            {
                pickUpGun.DorpWeapon();
            }
        }
    }

    public IEnumerator spawnObject(GameObject targetObject)
    {
        // Ellenхrizzьk, hogy van-e targetObject
        if (targetObject == null)
        {
            yield break;
        }

        // Ellenхrizzьk, hogy van-e quad
        if (quad == null)
        {
            yield break;
        }

        // Ellenхrizzьk, hogy a quadnak van-e MeshCollider komponense
        MeshCollider meshCollider = quad.GetComponent<MeshCollider>();
        if (meshCollider == null)
        {
            yield break;
        }

        // Reseteljьk a szбmlбlуkat
        destroyedTargets = 0;
        activeTargets.Clear();

        // Spawnoljuk a megadott szбmъ objektumot
        for (int i = 0; i < numberToSpawn; i++)
        {
            // Keresьnk egy megfelelх pozнciуt
            Vector3 spawnPosition = FindValidSpawnPosition(meshCollider);

            // Spawnoljuk az objektumot
            GameObject spawnedTarget = Instantiate(targetObject, spawnPosition, quad.transform.rotation);

            // Gyхzхdjьnk meg rуla, hogy a target-nek van Target komponense
            Target targetComponent = spawnedTarget.GetComponentInChildren<Target>();
            if (targetComponent == null)
            {
                targetComponent = spawnedTarget.AddComponent<Target>();
            }

            activeTargets.Add(spawnedTarget);

            // Vйletlenszerы vбrakozбs a kцvetkezх spawnolбsig
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