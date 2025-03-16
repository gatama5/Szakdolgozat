using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ScoreScreen : MonoBehaviour
{
    [Header("Navigation")]
    [SerializeField] private Button backToMainMenuButton;

    // Start is called before the first frame update
    void Start()
    {
        // Ellen�rizz�k, hogy van-e gomb hozz�rendelve
        if (backToMainMenuButton != null)
        {
            // Click esem�ny hozz�ad�sa a gombhoz
            backToMainMenuButton.onClick.AddListener(GoToMainMenu);
        }
        else
        {
            Debug.LogWarning("Back to main menu button is not assigned in the inspector!");
        }
    }

    // Visszat�r�s a f�men�be (0-�s szc�na)
    public void GoToMainMenu()
    {
        SceneManager.LoadScene(0);
    }
}