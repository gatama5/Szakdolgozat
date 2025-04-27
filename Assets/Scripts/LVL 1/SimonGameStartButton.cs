
using UnityEngine;
using System.Collections;
using TMPro;

public class SimonGameStartButton : MonoBehaviour
{
    [SerializeField] private SimonGameManager gm;
    [SerializeField] private SimonSaysButton[] buttons;
    [SerializeField] private AudioSource src;
    [SerializeField] private float start_delay = 3f;
    private bool canStart = true;

    [SerializeField] private bool showCountdown = true;
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private Canvas countdownCanvas;

    private void Start()
    {
        if (countdownCanvas != null)
        {
            countdownCanvas.enabled = false;
        }
    }

    void OnMouseDown()
    {
        if (!canStart || gm.isShowing) return;
        StartGame();
    }

    private void StartGame()
    {
        canStart = false;

        if (src != null) src.Play();

        ResetButtonColors();

        gm.isEnded = false;
        gm.ResetGame();

        StartCoroutine(CountdownAndStartGame());
    }

    private IEnumerator CountdownAndStartGame()
    {
        if (countdownCanvas != null)
        {
            countdownCanvas.enabled = true;
        }

        float remainingTime = start_delay;
        while (remainingTime > 0)
        {
            if (showCountdown && countdownText != null)
            {
                countdownText.text = Mathf.Ceil(remainingTime).ToString();
            }
            yield return new WaitForSeconds(1f);
            remainingTime -= 1f;
        }

        if (showCountdown && countdownText != null)
        {
            StartCoroutine(ClearTextAfterDelay(0f));
        }

        gm.StartCoroutine(gm.PlayGame());

        canStart = true;
    }

    private IEnumerator ClearTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (countdownText != null)
        {
            countdownText.text = "";
        }

        if (countdownCanvas != null)
        {
            countdownCanvas.enabled = false;
        }
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