using UnityEngine;

// A separate script to help fix the modal window state issues
public class ModalHelper : MonoBehaviour
{
    // Reference to the triggerZone (will find automatically if not set)
    [SerializeField] TriggerZoneForWindow triggerZone;
    [SerializeField] ui_c uiController;
    [SerializeField] ModalWindowPanel modalPanel;

    // Start is called before the first frame update
    void Start()
    {
        // Find references if not set
        if (triggerZone == null)
            triggerZone = FindObjectOfType<TriggerZoneForWindow>();

        if (uiController == null)
            uiController = FindObjectOfType<ui_c>();

        if (modalPanel == null)
            modalPanel = FindObjectOfType<ModalWindowPanel>();

        // Subscribe to modal window close button click
        if (modalPanel != null && modalPanel.apply_btn != null)
        {
            modalPanel.apply_btn.onClick.AddListener(OnModalClosed);
            Debug.Log("ModalResetHelper: Subscribed to modal close button");
        }
    }

    void Update()
    {
        // Emergency recovery shortcut (Ctrl+M)
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.M))
        {
            Debug.Log("ModalResetHelper: Emergency recovery triggered with Ctrl+M");
            ResetModalState();
        }
    }

    // Called when the modal window close button is clicked
    private void OnModalClosed()
    {
        Debug.Log("ModalResetHelper: Modal closed, resetting state");
        ResetModalState();
    }

    // Reset all modal-related state
    public void ResetModalState()
    {
        // Reset triggerZone
        if (triggerZone != null)
        {
            triggerZone.isOpen = false;
            Debug.Log("ModalResetHelper: Reset triggerZone.isOpen to false");
        }

        // Reset UI controller panel
        if (uiController != null && uiController.panel != null && uiController.panel.activeSelf)
        {
            uiController.Close();
            Debug.Log("ModalResetHelper: Closed uiController panel");
        }

        // Reset modal window
        if (modalPanel != null && modalPanel.modalWindow != null && modalPanel.modalWindow.activeSelf)
        {
            modalPanel.modalWindow.SetActive(false);
            Debug.Log("ModalResetHelper: Closed modal window");
        }

        Debug.Log("ModalResetHelper: All modal states reset");
    }
}

