using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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
    private bool timerActive = false;
    public bool playerInMaze = false;

    void Start()
    {
        // Kezdetben a gombok �s sz�veg rejtve
        if (buttonsParent != null)
            buttonsParent.SetActive(false);


        SetInitialButtonColors();
    }

    void Update()
    {
        if (timerActive)
        {
            timeInLabyrinth += Time.deltaTime;
        }
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

        // Id�z�t� ind�t�sa
        timerActive = true;
        timeInLabyrinth = 0f; // Id�z�t� null�z�sa amikor kezd�dik a j�t�k
        Debug.Log("Maze challenge started! Timer is running.");
    }

    public void OnButtonPress(GameObject pressedButton)
    {
        Debug.Log(pressedButton.name);
        pressedButton.GetComponent<MeshRenderer>().material.color = activated;
        buttonSound.Play();
        if (pressedButton == goodButton)
        {
            doorstate = true;
            timerActive = false;
            Debug.Log($"Gratul�lok! Teljes�tetted a feladatot! Id�d: {timeInLabyrinth:F2} m�sodperc");
        }
        else
        {
            doorstate = false;
            Debug.Log("Rossz gomb! Pr�b�ld �jra!");
        }
        //Destroy(pressedButton);
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
        timerActive = false;
        timeInLabyrinth = 0f;
        doorstate = false;
        SetInitialButtonColors();

        if (buttonsParent != null)
            buttonsParent.SetActive(false);
    }
}