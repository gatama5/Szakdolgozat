using UnityEngine;

public class PickUpGun : MonoBehaviour
{
    public GameObject gunOnPlayer;
    public GameObject gun_crosshair;
    public GameObject org_crosshair;
    public GameObject pickUpText;
    public bool isPickedUp = false;

    private bool isInRange = false;

    void Start()
    {
        gunOnPlayer.SetActive(false);
        pickUpText.SetActive(false);
    }

    void Update()
    {
        // Ha a jбtйkos hatуtбvon belьl van йs megnyomja az E gombot
        if (isInRange && !isPickedUp && Input.GetKeyDown(KeyCode.E))
        {
            PickUpWeapon();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInRange = true;
            pickUpText.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInRange = false;
            pickUpText.SetActive(false);
        }
    }

    private void PickUpWeapon()
    {
        this.gameObject.SetActive(false);
        org_crosshair.SetActive(false);
        gun_crosshair.SetActive(true);
        gunOnPlayer.SetActive(true);
        pickUpText.SetActive(false);
        isPickedUp = true;
    }

    public void DorpWeapon()
    {
        this.gameObject.SetActive(true);
        org_crosshair.SetActive(true);
        gun_crosshair.SetActive(false);
        gunOnPlayer.SetActive(false);
        pickUpText.SetActive(true);
        isPickedUp = false;
    }

}