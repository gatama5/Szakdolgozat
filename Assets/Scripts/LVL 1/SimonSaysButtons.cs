using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimonSaysButton : MonoBehaviour
{
    public Color defaultColor;
    [SerializeField] Color highlight;
    public Color gamend_color;
    [SerializeField] float btn_reset_delay = 0.25f;
    public float src_volume = 0.5f;
    AudioSource btn_sound;
    public float clickRate = 1f;
    private float next_clickTime = 0f;
    [SerializeField] SimonGameManager gameManager;



    public int ButtonIndex { get; set; }

    private void Awake()
    {
        btn_sound = GetComponent<AudioSource>();
    }

    private void Start()
    {
        btn_sound.volume = src_volume;
        ResetButton();
    }

     void OnMouseDown()
    {
        if (gameManager.isShowing == false)
        {
            if (Input.GetMouseButtonDown(0) && Time.time >= next_clickTime)
            {
                next_clickTime = Time.time + clickRate;
                gameManager.PlayerPick(ButtonIndex);
                PressButton();
            }        
        }
    }


    public void ResetButton() 
    {
        GetComponent<MeshRenderer>().material.color = defaultColor;
    }

    public void PressButton() 
    {
            btn_sound.Play();
            GetComponent<MeshRenderer>().material.color = highlight;
            Invoke(nameof(ResetButton), btn_reset_delay);
    }

}
