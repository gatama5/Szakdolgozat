//using UnityEngine;

//public class PickUpGun : MonoBehaviour
//{
//    public GameObject gunOnPlayer;
//    public GameObject gun_crosshair;
//    public GameObject org_crosshair;
//    public GameObject pickUpText;
//    public bool isPickedUp = false;

//    private bool isInRange = false;

//    void Start()
//    {
//        gunOnPlayer.SetActive(false);
//        pickUpText.SetActive(false);
//    }

//    void Update()
//    {
//        if (isInRange && !isPickedUp && Input.GetKeyDown(KeyCode.E))
//        {
//            PickUpWeapon();
//        }
//    }

//    private void OnTriggerEnter(Collider other)
//    {
//        if (other.CompareTag("Player"))
//        {
//            isInRange = true;
//            pickUpText.SetActive(true);
//        }
//    }

//    private void OnTriggerExit(Collider other)
//    {
//        if (other.CompareTag("Player"))
//        {
//            isInRange = false;
//            pickUpText.SetActive(false);
//        }
//    }

//    private void PickUpWeapon()
//    {
//        this.gameObject.SetActive(false);
//        org_crosshair.SetActive(false);
//        gun_crosshair.SetActive(true);
//        gunOnPlayer.SetActive(true);
//        pickUpText.SetActive(false);
//        isPickedUp = true;
//    }

//    public void DorpWeapon()
//    {
//        this.gameObject.SetActive(true);
//        org_crosshair.SetActive(true);
//        gun_crosshair.SetActive(false);
//        gunOnPlayer.SetActive(false);
//        pickUpText.SetActive(true);
//        isPickedUp = false;
//    }

//}

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
        // Ensure state is reset on start
        isInRange = false;
    }

    void Update()
    {
        // Add distance check as a failsafe
        if (isInRange && !isPickedUp)
        {
            // Double-check that player is actually in range using distance
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                float distance = Vector3.Distance(transform.position, player.transform.position);
                if (distance > 3.0f) // Adjust this distance based on your collider size
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
        isInRange = false; // Ensure we reset this flag
    }

    public void DropWeapon() // Fixed typo in function name
    {
        this.gameObject.SetActive(true);
        org_crosshair.SetActive(true);
        gun_crosshair.SetActive(false);
        gunOnPlayer.SetActive(false);
        isPickedUp = false;

        // Don't set pickup text or isInRange here
        // Only set them when player actually enters the trigger
    }
}