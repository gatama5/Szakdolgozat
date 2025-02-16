using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SimonGameManager : MonoBehaviour
{
    [SerializeField] SimonSaysButton[] buttons;
    [SerializeField] SimonGameStartButton startButton;
    [SerializeField] List<int> buttons_Order;
    [SerializeField] float pickDelay = 1f;

    [SerializeField] int pickNumber = 0;
    [SerializeField] SimonScores score;
    [SerializeField]  public bool isShowing = false;
    [SerializeField]  public bool isEnded = false;

    void Start()
    {
        //ResetGame();
        SetButtonIndex();
        //StartCoroutine("PlayGame");

        //for (int i = 0; i < buttons.Length; i++)
        //{
        //    buttons[i].enabled = false;
        //}
    }

    //private void Awake()
    //{
    //    SetButtonIndex();
    //}

    void SetButtonIndex() 
    {
        for (int i = 0; i < buttons.Length; i++) 
        {
            buttons[i].ButtonIndex = i;
        }
    }

    public IEnumerator PlayGame()
    {
        isShowing = true;
        pickNumber = 0;
        yield return new WaitForSeconds(pickDelay);
        foreach (var buttonIndex in buttons_Order)
        {
            buttons[buttonIndex].PressButton();
            yield return new WaitForSeconds(pickDelay);
        }
        PickRandomColor();
        isShowing = false;
    }

    void PickRandomColor()
    {
       int rnd = Random.Range(0, buttons.Length);
       buttons[rnd].PressButton();
        buttons_Order.Add(rnd);
    }

    public void PlayerPick(int btn_index)
    {
        if (!isShowing)
        {
            if (buttons_Order[pickNumber] == btn_index)
            {
                pickNumber++;
                if (pickNumber == buttons_Order.Count)
                {
                    score.Set(pickNumber); // update the player score
                    StartCoroutine("PlayGame");
                }
            }
            else
            {
                isEnded = true;
                //ResetGame();
                //StartCoroutine("PlayGame");
                //GameOver();
            }
        }
    }


    public void ResetGame()
    {
        score.CheckForNewHighscore();
        score.Set();
        buttons_Order.Clear();
    }

}
