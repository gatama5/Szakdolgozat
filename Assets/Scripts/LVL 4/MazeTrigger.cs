using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeTrigger : MonoBehaviour
{
    public ButtonsForMaze btn_mz;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player" && btn_mz.playerInMaze == false)
        {
            Debug.Log("Player entered the trigger zone!");
            btn_mz.playerInMaze = true;
            btn_mz.StartMazeChallenge();
        }
    }
}
