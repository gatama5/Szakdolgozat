
using UnityEngine;
using System.Collections;
using TMPro; // TextMeshPro namespace added

public class SimonGameStartButton : MonoBehaviour
{
    [SerializeField] private SimonGameManager gm;
    [SerializeField] private SimonSaysButton[] buttons;
    [SerializeField] private AudioSource src;
    [SerializeField] private float start_delay = 3f;
    private bool canStart = true;

    // New countdown UI elements
    [SerializeField] private bool showCountdown = true;
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private Canvas countdownCanvas;

    private void Start()
    {
        // Hide the countdown canvas at start
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
        // Temporarily disable the button
        canStart = false;

        // Play the sound
        if (src != null) src.Play();

        // Reset button colors
        ResetButtonColors();

        // Reset the game
        gm.isEnded = false;
        gm.ResetGame();

        // Start the countdown before starting the game
        StartCoroutine(CountdownAndStartGame());
    }

    private IEnumerator CountdownAndStartGame()
    {
        // Show countdown canvas if available
        if (countdownCanvas != null)
        {
            countdownCanvas.enabled = true;
        }

        // Perform countdown
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

        // Clear the countdown text
        if (showCountdown && countdownText != null)
        {
            StartCoroutine(ClearTextAfterDelay(0f));
        }

        // Start the game
        gm.StartCoroutine(gm.PlayGame());

        // Re-enable the start button
        canStart = true;
    }

    // Method to clear the countdown text after a delay
    private IEnumerator ClearTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (countdownText != null)
        {
            countdownText.text = "";
        }

        // Hide the countdown canvas
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