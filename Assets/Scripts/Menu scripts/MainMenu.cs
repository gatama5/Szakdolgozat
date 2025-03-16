using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{

    public Button settings_exit;

    public void TestMapPlay()
    {
        SceneManager.LoadScene(2);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ScoreScreen() 
    {
        SceneManager.LoadScene(1);
    }

    public void BackBtn() 
    {
        SceneManager.LoadScene(0);
    }

}
