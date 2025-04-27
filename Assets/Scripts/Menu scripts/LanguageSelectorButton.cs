using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LanguageSelectorButton : MonoBehaviour
{
    [Tooltip("A nyelv azonos�t�ja (0 = angol, 1 = magyar)")]
    [SerializeField] private int languageID = 0;

    private Button button;

    void Start()
    {
        button = GetComponent<Button>();

        if (button != null)
        {
            button.onClick.AddListener(ChangeLanguage);
        }
        else
        {
            Debug.LogError("A LanguageSelectorButton script-hez Button komponens sz�ks�ges!", this);
        }
    }

    private void ChangeLanguage()
    {
        if (LoacalisationManagerScript.Instance != null)
        {
            LoacalisationManagerScript.Instance.ChangeLocale(languageID);
        }
        else
        {
            Debug.LogError("LoacalisationManagerScript nem tal�lhat�! Ellen�rizd, hogy l�tezik-e a LoacalisationManager GameObject a sz�npadon!", this);
        }
    }
}