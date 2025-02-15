using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Diagnostics;
using System;

public class ButtonsForMaze : MonoBehaviour
{
    public GameObject[] badButtons;
    public GameObject goodButton;
    public Color deactivate = Color.yellow;
    public Color activated = Color.green;
    public AudioSource buttonSound;
    public GameObject buttonsParent;

    private bool doorstate = false;
    private float timeInLabyrinth = 0f;
    //private bool timerActive = false;
    public bool playerInMaze = false;
    Stopwatch sw = new Stopwatch();

    public TimeSpan score_time;

    void Start()
    {
        // Kezdetben a gombok �s sz�veg rejtve
        if (buttonsParent != null)
            buttonsParent.SetActive(false);


        SetInitialButtonColors();
    }

    void Update()
    {
        //if (timerActive)
        //{
        //    timeInLabyrinth += Time.deltaTime;
        //}
    }

    private void SetInitialButtonColors()
    {
        foreach (GameObject button in badButtons)
        {
            Renderer renderer = button.GetComponent<Renderer>();
            if (renderer != null)
                renderer.material.color = deactivate;
        }

        Renderer goodButtonRenderer = goodButton.GetComponent<Renderer>();
        if (goodButtonRenderer != null)
            goodButtonRenderer.material.color = deactivate;
    }

    public void StartMazeChallenge()
    {

        // Gombok megjelen�t�se
        if (buttonsParent != null)
            buttonsParent.SetActive(true);

        //// Id�z�t� ind�t�sa
        
        sw.Start();

        //timerActive = true;
        //timeInLabyrinth = 0f; // Id�z�t� null�z�sa amikor kezd�dik a j�t�k
        UnityEngine.Debug.Log("Maze challenge started! Timer is running.");
    }

    public void OnButtonPress(GameObject pressedButton)
    {
        if (playerInMaze)
        {
            UnityEngine.Debug.Log(pressedButton.name);
            pressedButton.GetComponent<MeshRenderer>().material.color = activated;
            buttonSound.Play();
            if (pressedButton == goodButton)
            {
                //doorstate = true;
                //timerActive = false;
                //UnityEngine.Debug.Log($"Gratul�lok! Teljes�tetted a feladatot! Id�d: {timeInLabyrinth:F2} m�sodperc");
                GoodButtonPress();

            }
            else
            {
                BadButtonPress(pressedButton);
                //doorstate = false;
                //UnityEngine.Debug.Log("Rossz gomb! Pr�b�ld �jra!");
            }
        }
    }

    public void GoodButtonPress() 
    {
        sw.Stop();
        TimeSpan timeTaken = sw.Elapsed;
        score_time = timeTaken;
        UnityEngine.Debug.Log($"Gratul�lok! Teljes�tetted a feladatot! Id�d: " + timeTaken + " m�sodperc");
        sw.Reset();
    }

    public void BadButtonPress(GameObject pressedButton)
    {
        UnityEngine.Debug.Log($"Rossz gomb keress tov�bb!");
        pressedButton.SetActive(false);
        //pressedButton.active = false;
    }

    public bool GetDoorState()
    {
        return doorstate;
    }

    public float GetCompletionTime()
    {
        return timeInLabyrinth;
    }

    // Reset f�ggv�ny hozz�ad�sa, ha sz�ks�ges �jrakezdeni a j�t�kot
    public void ResetMaze()
    {
        playerInMaze = false;
        //timerActive = false;
        timeInLabyrinth = 0f;
        doorstate = false;
        SetInitialButtonColors();

        if (buttonsParent != null)
            buttonsParent.SetActive(false);
    }
}