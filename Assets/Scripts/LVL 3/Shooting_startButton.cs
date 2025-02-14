using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooting_startButton : MonoBehaviour
{
    public ObjectSpawner_1place obj_s_1p;
    public GameObject trg;
    public PickUpGun pug;
    public AudioSource src;
    [SerializeField] public float start_delay = 3f;


    private void OnMouseDown()
    {
        src.Play();
        if (pug.isPickedUp)
        {
            StartCoroutine(StartPlay());
        }
        else
        {
            Debug.Log("Kérem vegye fel a fegyvert a kezdéshez!");
        }
    }

    public IEnumerator StartPlay()
    {
        obj_s_1p.StartCoroutine(obj_s_1p.spawnObject(trg));
        yield return new WaitForSeconds(start_delay);
    }

    //public GameObject GetPressedButton()
    //{
    //    return gameObject;
    //}
}
