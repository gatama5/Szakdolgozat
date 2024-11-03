using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ObjectSpawner : MonoBehaviour
{
    public int numberToSpawn = 5;
    public GameObject target;
    public GameObject quad;
    public float spawnDelay = 2.0f; // spawnolások között eltelt idõ (mp-ben)

    void Start()
    {
        //asszinkron késleltetett spawn ráta
        StartCoroutine(spawnObject(target)); 

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator spawnObject(GameObject obj)
    {
        MeshCollider collider = quad.GetComponent<MeshCollider>();
        float fit_x, fit_y;
        Vector3 fit_quad;

        for (int i = 0; i < numberToSpawn; i++)
        {
            fit_x = Random.Range(collider.bounds.min.x, collider.bounds.max.x);
            fit_y = Random.Range(collider.bounds.min.y, collider.bounds.max.y);
            fit_quad = new Vector3(fit_x, fit_y, quad.transform.position.z);

            Instantiate(obj, fit_quad, quad.transform.rotation);
            yield return new WaitForSeconds(spawnDelay);
        }
    }

}
