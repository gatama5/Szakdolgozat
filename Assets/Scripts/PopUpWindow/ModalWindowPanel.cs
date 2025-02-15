using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModalWindowPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI title_text;
    [SerializeField] private TextMeshProUGUI content_text;
    [SerializeField] private TextMeshProUGUI apply_btn_text;
    [SerializeField] Button apply_btn;
    //[SerializeField] Canvas panel;

    private Action onApplyAction;

    public void Confirm()
    {
        onApplyAction?.Invoke();
        //panel.gameObject.SetActive(false);
    }

    public void ShowAsHero(string title, string message, Action confirmAction) 
    {
        title_text.text = title;
        content_text.text = message;
        onApplyAction = confirmAction;

    }

}
