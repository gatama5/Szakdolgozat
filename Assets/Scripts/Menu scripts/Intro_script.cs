
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class Intro_script : MonoBehaviour
{
    [SerializeField] private RawImage videoRawImage;
    [SerializeField] private float videoLength = 10f;

    // �j v�ltoz� a vide� gyors�t�s�hoz
    [Range(0.5f, 2.0f)]
    [Tooltip("A vide� lej�tsz�si sebess�ge (1.0 = norm�l, 1.5 = m�sf�lszeres, 2.0 = dupla sebess�g)")]
    [SerializeField] private float videoSpeedMultiplier = 1.0f;

    // Opcion�lis VideoPlayer referencia, ha van
    [SerializeField] private VideoPlayer videoPlayer;

    void Start()
    {
        // Ha van VideoPlayer, akkor be�ll�tjuk a sebess�get
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

        // M�dos�tott v�rakoz�si id� a gyors�t�s alapj�n
        float adjustedWaitTime = videoLength / videoSpeedMultiplier;
        Debug.Log($"V�rakoz�si id�: {adjustedWaitTime} m�sodperc ({videoLength} / {videoSpeedMultiplier})");

        yield return new WaitForSeconds(adjustedWaitTime);

        videoRawImage.gameObject.SetActive(false);

        SceneManager.LoadScene(3);
    }
}