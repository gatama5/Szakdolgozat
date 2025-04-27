
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
        isInRange = false;
    }

    void Update()
    {
        if (isInRange && !isPickedUp)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                float distance = Vector3.Distance(transform.position, player.transform.position);
                if (distance > 3.0f)
                {
                    isInRange = false;
                    pickUpText.SetActive(false);
                    return;
                }
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                PickUpWeapon();
            }
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
        isInRange = false;
    }

    public void DropWeapon()
    {
        this.gameObject.SetActive(true);
        org_crosshair.SetActive(true);
        gun_crosshair.SetActive(false);
        gunOnPlayer.SetActive(false);
        isPickedUp = false;

    }
}