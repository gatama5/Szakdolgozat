using UnityEngine;

public class TriggerZoneForWindow : MonoBehaviour
{
    private Vector3 fixedPosition;
    private Quaternion fixedRotation;
    public FPS_Controller playerController;
    [SerializeField] public GameObject player;

    [SerializeField] public Canvas megjelenit;
    [SerializeField] public bool isOpen = false;

    public int TriggerZoneEnter = 0;


    public void Awake()
    {
        megjelenit.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (TriggerZoneEnter == 0)
        {
            if (other.CompareTag("Player"))
            {
                player = other.gameObject;
                playerController = player.GetComponent<FPS_Controller>();

                if (playerController != null)
                {
                    fixedPosition = player.transform.position;
                    fixedRotation = player.transform.rotation;

                    playerController.canMove = false;

                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;

                    isOpen = true;
                    megjelenit.enabled = true;
                    TriggerZoneEnter++;

                }
            }
        }
    }

    private void Update()
    {
        if (playerController != null && !playerController.canMove)
        {
            player.transform.position = fixedPosition;

        }
    }

    public void OnApplyPressed()
    {
        if (playerController != null)
        {
            playerController.canMoveAgain();
        }
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isOpen = false;
    }
}

