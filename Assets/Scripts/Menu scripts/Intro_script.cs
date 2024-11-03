using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Intro_script : MonoBehaviour
{
    [SerializeField] private RawImage videoRawImage;
    [SerializeField] private float videoLength = 10f;

    void Start()
    {
        StartCoroutine(PlayVideoAndLoadNextScene());
    }

    private IEnumerator PlayVideoAndLoadNextScene()
    {
        videoRawImage.gameObject.SetActive(true);
        yield return new WaitForSeconds(videoLength);
        videoRawImage.gameObject.SetActive(false);
        
        SceneManager.LoadScene(3);
    }
}