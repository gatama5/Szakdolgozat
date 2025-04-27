
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialBtnScript : MonoBehaviour
{
    [SerializeField] GameObject panel;
    public bool panelIsActive = false;
    [SerializeField] FPS_Controller playerController;
    [SerializeField] TriggerZoneForWindow triggerZone; 
    [SerializeField] ui_c uiController;
    public float delay = 2f;
    private bool inputLocked = false; 
    private float lastInputLockTime = 0f;


    void Awake()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }


        if (uiController == null)
        {
            uiController = FindObjectOfType<ui_c>();
        }

        if (triggerZone == null)
        {
            triggerZone = FindObjectOfType<TriggerZoneForWindow>();
        }

        ModalWindowPanel modalPanel = FindObjectOfType<ModalWindowPanel>();
        if (modalPanel != null && modalPanel.apply_btn != null)
        {
            modalPanel.apply_btn.onClick.AddListener(OnModalClosed);
        }
    }

    private void OnModalClosed()
    {
        StartCoroutine(DelayedStateReset());
    }

    private IEnumerator DelayedStateReset()
    {

        yield return null;


        if (triggerZone != null)
        {
            triggerZone.isOpen = false;
        }


        inputLocked = false;
    }

    void Start()
    {
        if (playerController == null)
        {
            playerController = FindObjectOfType<FPS_Controller>();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("T key was pressed");
        }


        if (inputLocked && Time.time - lastInputLockTime > 5.0f)
        {
            Debug.Log("TutorialBtnScript: Input lock timeout - forcing unlock");
            ForceUnlockInput();
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            bool isModalActive = CheckIfModalActive();

            Debug.Log("TutorialBtnScript: Processing T key press, current panel state: " + panelIsActive);
            Debug.Log("TutorialBtnScript: Modal active check: " + isModalActive);

            if (!isModalActive && !inputLocked)
            {
                Debug.Log("TutorialBtnScript: Calling TogglePanel()");
                TogglePanel();
            }
            else
            {
                Debug.Log("TutorialBtnScript: Cannot toggle panel - modal is active or input is locked");
            }
        }

        if (Input.GetKeyDown(KeyCode.R) && Input.GetKey(KeyCode.LeftShift))
        {
            Debug.Log("TutorialBtnScript: Reset key combination detected");
            ResetState();
        }

        if (Input.GetKeyDown(KeyCode.I) && Input.GetKey(KeyCode.LeftShift))
        {
            Debug.Log($"TutorialBtnScript: Status - inputLocked: {inputLocked}, panelIsActive: {panelIsActive}, panel active: {(panel != null ? panel.activeSelf : false)}");
        }
    }

    private bool CheckIfModalActive()
    {
        bool result = false;

        bool triggerZoneOpen = triggerZone != null && triggerZone.isOpen;

        ModalWindowPanel modalPanel = FindObjectOfType<ModalWindowPanel>();
        bool modalWindowActive = false;

        if (modalPanel != null && modalPanel.modalWindow != null)
        {
            modalWindowActive = modalPanel.modalWindow.activeSelf;
        }

        bool uiControllerPanelActive = uiController != null && uiController.panel != null && uiController.panel.activeSelf;

        Debug.Log($"TutorialBtnScript: Modal check - triggerZone.isOpen: {triggerZoneOpen}, modalWindow.activeSelf: {modalWindowActive}, uiController.panel.activeSelf: {uiControllerPanelActive}");


        if (triggerZoneOpen)
        {
            Debug.Log("TutorialBtnScript: Player is in trigger zone, modal is active");
            result = true;
        }
        else
        {
            Debug.Log("TutorialBtnScript: Player is NOT in trigger zone, modal is NOT active");
            result = false;

            if (modalWindowActive || uiControllerPanelActive)
            {
                Debug.Log("TutorialBtnScript: Inconsistent state detected - modal appears active but player not in trigger zone");
            }
        }

        Debug.Log("TutorialBtnScript: Modal active check result: " + result);

        return result;
    }


    private void TogglePanel()
    {
        if (panel == null)
        {
            Debug.LogError("TutorialBtnScript: Cannot toggle panel - panel reference is null");
            return;
        }

        inputLocked = true; 
        lastInputLockTime = Time.time;

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

    public void ResetState()
    {
        Debug.Log("TutorialBtnScript: Resetting all state values");

        inputLocked = false;

        if (panel != null)
        {
            panelIsActive = panel.activeSelf;
            Debug.Log("TutorialBtnScript: Panel active state reset to: " + panelIsActive);
        }

        if (triggerZone != null)
        {
            triggerZone.isOpen = false;
            Debug.Log("TutorialBtnScript: TriggerZone.isOpen reset to false");
        }


        if (uiController != null && uiController.panel != null)
        {
            Debug.Log("TutorialBtnScript: UI Controller panel state: " + uiController.panel.activeSelf);
        }

        if (playerController != null)
        {
            playerController.canMove = !panelIsActive;
            Debug.Log("TutorialBtnScript: Player movement set to: " + playerController.canMove);
        }
    }

    public void ForceUnlockInput()
    {
        inputLocked = false;
        Debug.Log("TutorialBtnScript: Input forcibly unlocked");
    }
}