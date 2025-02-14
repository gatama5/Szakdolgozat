using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeTrigger : MonoBehaviour
{
    public ButtonsForMaze btn_mz;
    public int mazeTriggerTimes = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (mazeTriggerTimes == 0 && other.gameObject.tag == "Player" && btn_mz.playerInMaze == false)
        {
            Debug.Log("Player entered the trigger zone!");
            mazeTriggerTimes++;
            btn_mz.playerInMaze = true;
            btn_mz.StartMazeChallenge();
        }

        if (mazeTriggerTimes > 0 && other.gameObject.tag == "Player" && btn_mz.playerInMaze == true)
        {
            Debug.Log("Keress tovább");
        }

    }
}
