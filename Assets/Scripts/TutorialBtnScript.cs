
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialBtnScript : MonoBehaviour
{
    [SerializeField] GameObject panel;
    public bool panelIsActive = false;
    [SerializeField] FPS_Controller playerController;
    [SerializeField] TriggerZoneForWindow triggerZone; // Reference to check if modal window is active
    // Added reference to ui_c to directly check if modal window is active
    [SerializeField] ui_c uiController;
    public float delay = 2f;
    private bool inputLocked = false; // Flag to prevent input processing during transitions
    private float lastInputLockTime = 0f; // Track when input was last locked

    // Start is called before the first frame update
    void Awake()
    {



        if (panel != null)
        {
            panel.SetActive(false);
        }


        // If uiController reference is not set, try to find it in the scene
        if (uiController == null)
        {
            uiController = FindObjectOfType<ui_c>();
        }

        // If triggerZone reference is not set, try to find it in the scene
        if (triggerZone == null)
        {
            triggerZone = FindObjectOfType<TriggerZoneForWindow>();

        }



        // Keress ModalWindowPanel-t, �s �ll�ts be listener-t a bez�r�sra
        ModalWindowPanel modalPanel = FindObjectOfType<ModalWindowPanel>();
        if (modalPanel != null && modalPanel.apply_btn != null)
        {

            modalPanel.apply_btn.onClick.AddListener(OnModalClosed);
        }
    }

    // Modal ablak bez�r�sa ut�n megh�v�dik
    private void OnModalClosed()
    {

        StartCoroutine(DelayedStateReset());
    }

    // K�sleltetett �llapot vissza�ll�t�s
    private IEnumerator DelayedStateReset()
    {
        // V�rjunk egy frame-et, hogy minden m�s script lefusson
        yield return null;

        // Most �ll�tsuk vissza az �llapotokat
        if (triggerZone != null)
        {

            triggerZone.isOpen = false;
        }

        // Gy�z�dj meg r�la, hogy az inputLocked felold�dik
        inputLocked = false;


    }

    // Additional reference check on Start
    void Start()
    {


        if (playerController == null)
        {

            playerController = FindObjectOfType<FPS_Controller>();

        }
    }

    // Update is called once per frame
    void Update()
    {
        // Force unlock input if stuck for too long (5 seconds)
        // This prevents the script from getting permanently locked
        if (inputLocked && Time.time - lastInputLockTime > 5.0f)
        {

            ForceUnlockInput();
        }

        if (inputLocked)
        {
            // Skip input processing if locked
            return;
        }

        // Check for T key press directly
        if (Input.GetKeyDown(KeyCode.T))
        {
            
            // First check if any modal window is active
            bool isModalActive = CheckIfModalActive();


            if (!isModalActive)
            {
                Debug.Log("TutorialBtnScript: Processing T key press, current panel state: " + panelIsActive);
                TogglePanel();
            }

        }

        // Debug key for resetting state (R key)
        if (Input.GetKeyDown(KeyCode.R) && Input.GetKey(KeyCode.LeftShift))
        {

            ResetState();
        }
    }

    // Separate method to check modal status
    private bool CheckIfModalActive()
    {
        bool result = false;

        // Debug info
        bool triggerZoneOpen = triggerZone != null && triggerZone.isOpen;
        bool uiControllerPanelActive = uiController != null && uiController.panel != null && uiController.panel.activeSelf;

        Debug.Log($"TutorialBtnScript: Checking modal status - triggerZone.isOpen: {triggerZoneOpen}, uiController.panel.activeSelf: {uiControllerPanelActive}, panelIsActive: {panelIsActive}, panel.activeSelf: {(panel != null ? panel.activeSelf : false)}");

        // JAV�T�S: Ellen�rizz�k a modalWindow-t k�zvetlen�l is
        ModalWindowPanel modalPanel = FindObjectOfType<ModalWindowPanel>();
        bool modalWindowActive = false;

        if (modalPanel != null && modalPanel.modalWindow != null)
        {
            modalWindowActive = modalPanel.modalWindow.activeSelf;
            Debug.Log($"TutorialBtnScript: ModalWindowPanel.modalWindow.activeSelf: {modalWindowActive}");
        }

        // Check through triggerZone - csak akkor akt�v, ha a j�t�kos a z�n�ban van
        if (triggerZoneOpen)
        {
            Debug.Log("TutorialBtnScript: TriggerZone is marked as open");

            // Ha a modalWindow nem akt�v, de triggerZone.isOpen = true, akkor jav�tsuk
            if (!modalWindowActive)
            {
                Debug.Log("TutorialBtnScript: TriggerZone incorrectly marked as open - fixing");
                triggerZone.isOpen = false;
            }
            else
            {
                result = true;
            }
        }

        // Check through uiController (panel active status)
        if (uiControllerPanelActive)
        {
            Debug.Log("TutorialBtnScript: UI Controller panel is active and visible");

            // Ha a modalWindow nem akt�v, de uiController.panel akt�v, akkor jav�tsuk
            if (!modalWindowActive)
            {
                Debug.Log("TutorialBtnScript: UI Controller panel incorrectly active - fixing");
                uiController.Close();
            }
            else
            {
                result = true;
            }
        }

        if (result)
        {
            Debug.Log("TutorialBtnScript: Modal is active");
        }
        else
        {
            Debug.Log("TutorialBtnScript: No active modal windows detected");
        }

        return result;
    }

    // Toggle panel status
    private void TogglePanel()
    {
        // Safety check for panel reference
        if (panel == null)
        {
            Debug.LogError("TutorialBtnScript: Cannot toggle panel - panel reference is null");
            return;
        }

        inputLocked = true; // Lock input during transition
        lastInputLockTime = Time.time; // Record when we locked the input

        if (!panelIsActive)
        {
            Debug.Log("TutorialBtnScript: Starting Show coroutine");
            StartCoroutine(Show());
        }
        else
        {
            Debug.Log("TutorialBtnScript: Starting Hide coroutine");
            StartCoroutine(Hide());
        }
    }

    // Direct toggle method for testing through UI or other scripts
    public void TestTogglePanel()
    {
        Debug.Log("TutorialBtnScript: Test toggle called");

        if (panel == null)
        {
            Debug.LogError("TutorialBtnScript: Cannot test toggle - panel reference is null");
            return;
        }

        panel.SetActive(!panel.activeSelf);
        panelIsActive = panel.activeSelf;

        Debug.Log("TutorialBtnScript: Panel active state set to: " + panelIsActive);

        if (playerController != null)
        {
            playerController.canMove = !panelIsActive;
            Debug.Log("TutorialBtnScript: Player movement set to: " + playerController.canMove);
        }
    }

    public IEnumerator Show()
    {
        Debug.Log("TutorialBtnScript: Show - Setting panel active");
        panel.SetActive(true);
        panelIsActive = true;

        if (playerController != null)
        {
            playerController.canMove = false;
            Debug.Log("TutorialBtnScript: Disabled player movement");
        }
        else
        {
            Debug.LogWarning("TutorialBtnScript: PlayerController is null, cannot disable movement");
        }

        yield return new WaitForSeconds(delay);
        Debug.Log("TutorialBtnScript: Show - Unlocking input after delay");
        inputLocked = false;
    }

    public IEnumerator Hide()
    {
        Debug.Log("TutorialBtnScript: Hide - Setting panel inactive");
        panel.SetActive(false);
        panelIsActive = false;

        if (playerController != null)
        {
            playerController.canMove = true;
            Debug.Log("TutorialBtnScript: Enabled player movement");
        }
        else
        {
            Debug.LogWarning("TutorialBtnScript: PlayerController is null, cannot enable movement");
        }

        yield return new WaitForSeconds(delay);
        Debug.Log("TutorialBtnScript: Hide - Unlocking input after delay");
        inputLocked = false;
    }

    // �j met�dus az �llapot vissza�ll�t�s�hoz
    public void ResetState()
    {
        Debug.Log("TutorialBtnScript: Resetting all state values");

        // �ll�tsuk vissza az �sszes �llapotjelz�t
        inputLocked = false;

        // A panel �llapot�t az aktu�lis activeself �rt�k�hez igaz�tjuk
        if (panel != null)
        {
            panelIsActive = panel.activeSelf;
            Debug.Log("TutorialBtnScript: Panel active state reset to: " + panelIsActive);
        }

        // Ellen�rizz�k �s vissza�ll�tjuk a triggerZone �llapot�t
        if (triggerZone != null)
        {
            triggerZone.isOpen = false;
            Debug.Log("TutorialBtnScript: TriggerZone.isOpen reset to false");
        }

        // Ellen�rizz�k �s vissza�ll�tjuk az uiController �llapot�t
        if (uiController != null && uiController.panel != null)
        {
            Debug.Log("TutorialBtnScript: UI Controller panel state: " + uiController.panel.activeSelf);
        }

        // J�t�kos mozg�s�t vissza�ll�tjuk, ha a panel nem akt�v
        if (playerController != null)
        {
            playerController.canMove = !panelIsActive;
            Debug.Log("TutorialBtnScript: Player movement set to: " + playerController.canMove);
        }
    }

    // K�nyszer�tett input felold�s
    public void ForceUnlockInput()
    {
        inputLocked = false;
        Debug.Log("TutorialBtnScript: Input forcibly unlocked");
    }
}