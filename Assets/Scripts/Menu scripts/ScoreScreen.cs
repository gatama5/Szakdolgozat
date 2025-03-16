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
        // Ellenõrizzük, hogy van-e gomb hozzárendelve
        if (backToMainMenuButton != null)
        {
            // Click esemény hozzáadása a gombhoz
            backToMainMenuButton.onClick.AddListener(GoToMainMenu);
        }
        else
        {
            Debug.LogWarning("Back to main menu button is not assigned in the inspector!");
        }
    }

    // Visszatérés a fõmenübe (0-ás szcéna)
    public void GoToMainMenu()
    {
        SceneManager.LoadScene(0);
    }
}