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
    // Start is called before the first frame update
    void Start()
    {
        gunOnPlayer.SetActive(false);
        pickUpText.SetActive(false);
    }

    private void OnTriggerStay(Collider other)
    {
        pickUpText.SetActive(true);

        if (other.gameObject.tag == "Player")
        {
            if (Input.GetKeyUp(KeyCode.E))
            {
                this.gameObject.SetActive(false);
                org_crosshair.SetActive(false);
                gun_crosshair.SetActive(true);
                gunOnPlayer.SetActive(true);
                pickUpText.SetActive(false);
            }   
        }
    }

    private void OnTriggerExit(Collider other)
    {
        pickUpText.SetActive(false);
    }

}
