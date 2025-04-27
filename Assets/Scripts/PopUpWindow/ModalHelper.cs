using UnityEngine;

public class ModalHelper : MonoBehaviour
{
    [SerializeField] TriggerZoneForWindow triggerZone;
    [SerializeField] ui_c uiController;
    [SerializeField] ModalWindowPanel modalPanel;

    void Start()
    {
        if (triggerZone == null)
            triggerZone = FindObjectOfType<TriggerZoneForWindow>();

        if (uiController == null)
            uiController = FindObjectOfType<ui_c>();

        if (modalPanel == null)
            modalPanel = FindObjectOfType<ModalWindowPanel>();

        if (modalPanel != null && modalPanel.apply_btn != null)
        {
            modalPanel.apply_btn.onClick.AddListener(OnModalClosed);
            Debug.Log("ModalResetHelper: Subscribed to modal close button");
        }
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.M))
        {
            Debug.Log("ModalResetHelper: Emergency recovery triggered with Ctrl+M");
            ResetModalState();
        }
    }

    private void OnModalClosed()
    {
        Debug.Log("ModalResetHelper: Modal closed, resetting state");
        ResetModalState();
    }

    public void ResetModalState()
    {

        if (triggerZone != null)
        {
            triggerZone.isOpen = false;
            Debug.Log("ModalResetHelper: Reset triggerZone.isOpen to false");
        }


        if (uiController != null && uiController.panel != null && uiController.panel.activeSelf)
        {
            uiController.Close();
            Debug.Log("ModalResetHelper: Closed uiController panel");
        }


        if (modalPanel != null && modalPanel.modalWindow != null && modalPanel.modalWindow.activeSelf)
        {
            modalPanel.modalWindow.SetActive(false);
            Debug.Log("ModalResetHelper: Closed modal window");
        }

        Debug.Log("ModalResetHelper: All modal states reset");
    }
}

