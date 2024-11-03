using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeButtonPress : MonoBehaviour
{
    public ButtonsForMaze btn_mz;


    private void OnMouseDown()
    {
        btn_mz.OnButtonPress(GetPressedButton());
    }

    public GameObject GetPressedButton()
    {
        return gameObject;
    }
}
