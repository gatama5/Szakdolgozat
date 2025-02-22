//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class SimonGameStartButton : MonoBehaviour
//{
//     public SimonGameManager gm;
//     public SimonSaysButton[] buttons;
//     public AudioSource src;
//     [SerializeField] public float start_delay = 3f;

//    void OnMouseDown()
//    {
//        //for (int i = 0; i < buttons.Length; i++)
//        //{
//        //    buttons[i].GetComponent<MeshRenderer>().material.color = buttons[i].defaultColor;
//        //}
//        src.Play();
//        gm.isEnded = false;
//        gm.ResetGame();
//        gm.StartCoroutine(gm.PlayGame());
//    }
//}

using UnityEngine;

public class SimonGameStartButton : MonoBehaviour
{
    [SerializeField] private SimonGameManager gm;
    [SerializeField] private SimonSaysButton[] buttons;
    [SerializeField] private AudioSource src;
    [SerializeField] private float start_delay = 3f;

    private bool canStart = true;

    void OnMouseDown()
    {
        if (!canStart || gm.isShowing) return;

        StartGame();
    }

    private void StartGame()
    {
        // Átmenetileg letiltjuk a gombot
        canStart = false;

        // Lejátszuk a hangot
        if (src != null) src.Play();

        // Visszaállítjuk az alapszíneket
        ResetButtonColors();

        // Elindítjuk a játékot
        gm.isEnded = false;
        gm.ResetGame();

        // Késleltetett játékindítás
        StartCoroutine(DelayedGameStart());
    }

    private System.Collections.IEnumerator DelayedGameStart()
    {
        yield return new WaitForSeconds(start_delay);

        // Start the first round
        gm.StartCoroutine(gm.PlayGame());

        // Újra engedélyezzük a gombot
        canStart = true;
    }

    private void ResetButtonColors()
    {
        if (buttons != null)
        {
            foreach (var button in buttons)
            {
                if (button != null)
                {
                    button.ResetButton();
                }
            }
        }
    }
}