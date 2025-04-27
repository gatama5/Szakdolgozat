
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class Intro_script : MonoBehaviour
{
    [SerializeField] private RawImage videoRawImage;
    [SerializeField] private float videoLength = 10f;

    [Range(0.5f, 2.0f)]
    [Tooltip("A vide� lej�tsz�si sebess�ge (1.0 = norm�l, 1.5 = m�sf�lszeres, 2.0 = dupla sebess�g)")]
    [SerializeField] private float videoSpeedMultiplier = 1.0f;

    [SerializeField] private VideoPlayer videoPlayer;

    void Start()
    {
        if (videoPlayer != null)
        {
            videoPlayer.playbackSpeed = videoSpeedMultiplier;
            Debug.Log($"Vide� lej�tsz�si sebess�g be�ll�tva: {videoSpeedMultiplier}x");
        }

        StartCoroutine(PlayVideoAndLoadNextScene());
    }

    private IEnumerator PlayVideoAndLoadNextScene()
    {
        videoRawImage.gameObject.SetActive(true);
        float adjustedWaitTime = videoLength / videoSpeedMultiplier;
        Debug.Log($"V�rakoz�si id�: {adjustedWaitTime} m�sodperc ({videoLength} / {videoSpeedMultiplier})");

        yield return new WaitForSeconds(adjustedWaitTime);

        videoRawImage.gameObject.SetActive(false);

        SceneManager.LoadScene(3);
    }
}