using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PickUpGun : MonoBehaviour
{
    public GameObject gunOnPlayer;
    public GameObject gun_crosshair;
    public GameObject org_crosshair;
    public GameObject pickUpText;

    public bool isPickedUp = false;

    public object InputSystem { get; private set; }

    void Start()
    {
        gunOnPlayer.SetActive(false);
        pickUpText.SetActive(false);
    }

    private void OnTriggerStay(Collider other)
    {
        pickUpText.SetActive(true);

        if(!isPickedUp)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                this.gameObject.SetActive(false);
                org_crosshair.SetActive(false);
                gun_crosshair.SetActive(true);
                gunOnPlayer.SetActive(true);
                pickUpText.SetActive(false);
                isPickedUp=true;
            }   
        }
    }

    private void OnTriggerExit(Collider other)
    {
        pickUpText.SetActive(false);
    }

}
