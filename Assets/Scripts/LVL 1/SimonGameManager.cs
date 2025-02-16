using System;
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

    //[SerializeField] public GameObject door;
    //[SerializeField] public GameObject doorknob;


    //public Tuple<Vector3, Quaternion> door_start_pos =
    // new Tuple<Vector3, Quaternion>(
    //     new Vector3(13.21f, 2.32f, 6.26f),
    //     new Quaternion(0, -0.707103193f, 0, 0.707110405f)
    // );

    //public Tuple<Vector3, Quaternion> door_open_pos =
    //    new Tuple<Vector3, Quaternion>(
    //         new Vector3(16.44f, 2.32f, 3.75f),
    //         new Quaternion(0, -1, 0, 5.30f)
    //   );


    //public Tuple<Vector3, Quaternion> door_knob_start = new Tuple<Vector3, Quaternion>(new Vector3(10.72f, 6.48f, 6.26f), new Quaternion(0, -0.70f, 0, 0.70f));
    //public Tuple<Vector3, Quaternion> door_knob_open = new Tuple<Vector3, Quaternion>(new Vector3(16.5f, 6.4000001f, 0.109999999f), new Quaternion(0, -0.00617052522f, 0, 0.999981046f));


    void Start()
    {
        //door.transform.position = door_start_pos.Item1;
        //door.transform.rotation = door_start_pos.Item2;
        //doorknob.transform.position = door_knob_start.Item1;
        //doorknob.transform.rotation = door_knob_start.Item2;
        

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
        int rnd = UnityEngine.Random.Range(0, buttons.Length);
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

                //door.transform.position = door_open_pos.Item1;
                //door.transform.rotation = door_open_pos.Item2;
                //doorknob.transform.position = door_knob_open.Item1;
                //doorknob.transform.rotation = door_knob_open.Item2;
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
