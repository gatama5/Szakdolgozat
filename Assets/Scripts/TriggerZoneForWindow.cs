using UnityEngine;

public class TriggerZoneForWindow : MonoBehaviour
{
    private Vector3 fixedPosition;
    private Quaternion fixedRotation;
    public FPS_Controller playerController;
    [SerializeField] public GameObject player;

    //[SerializeField] public ui_c panel;
    [SerializeField] public Canvas megjelenit;
    [SerializeField] public bool isOpen = false;

    public int TriggerZoneEnter = 0;


    public void Awake()
    {
        megjelenit.enabled = false;
        //panel.Close();
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
                    // Fix�ljuk a poz�ci�t �s forg�st
                    fixedPosition = player.transform.position;
                    fixedRotation = player.transform.rotation;

                    // Letiltjuk a mozg�st
                    playerController.canMove = false;

                    // Egeret l�that�v� tessz�k �s letiltjuk a kurzor r�gz�t�s�t
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;

                    // Megjelen�tj�k a Modal Window-t
                    isOpen = true;
                    megjelenit.enabled = true;
                    TriggerZoneEnter++;
                    //panel.Show();
                    //ui_c.Instance.modalwindow.ShowAsHero();
                }
            }
        }
    }

    private void Update()
    {
        if (playerController != null && !playerController.canMove)
        {
            // Fix�ljuk a poz�ci�t �s forg�st am�g a Modal Window akt�v
            player.transform.position = fixedPosition;
            //player.transform.rotation = fixedRotation;
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

