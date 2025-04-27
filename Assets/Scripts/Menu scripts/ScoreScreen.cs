using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ScoreScreen : MonoBehaviour
{
    [Header("Navigation")]
    [SerializeField] private Button backToMainMenuButton;

    void Start()
    {
        if (backToMainMenuButton != null)
        {
            backToMainMenuButton.onClick.AddListener(GoToMainMenu);
        }
        else
        {
            Debug.LogWarning("Back to main menu button is not assigned in the inspector!");
        }
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene(0);
    }
}