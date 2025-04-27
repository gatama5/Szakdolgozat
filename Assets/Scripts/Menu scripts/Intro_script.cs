
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
    [Tooltip("A videó lejátszási sebessége (1.0 = normál, 1.5 = másfélszeres, 2.0 = dupla sebesség)")]
    [SerializeField] private float videoSpeedMultiplier = 1.0f;

    [SerializeField] private VideoPlayer videoPlayer;

    void Start()
    {
        if (videoPlayer != null)
        {
            videoPlayer.playbackSpeed = videoSpeedMultiplier;
            Debug.Log($"Videó lejátszási sebesség beállítva: {videoSpeedMultiplier}x");
        }

        StartCoroutine(PlayVideoAndLoadNextScene());
    }

    private IEnumerator PlayVideoAndLoadNextScene()
    {
        videoRawImage.gameObject.SetActive(true);
        float adjustedWaitTime = videoLength / videoSpeedMultiplier;
        Debug.Log($"Várakozási idõ: {adjustedWaitTime} másodperc ({videoLength} / {videoSpeedMultiplier})");

        yield return new WaitForSeconds(adjustedWaitTime);

        videoRawImage.gameObject.SetActive(false);

        SceneManager.LoadScene(3);
    }
}