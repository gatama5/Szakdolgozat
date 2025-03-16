using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialBtnScript : MonoBehaviour
{
    [SerializeField] GameObject panel;
    public bool panelIsActive = false;
    [SerializeField] FPS_Controller playerController;
    public float delay = 2f;

    // Start is called before the first frame update
    void Awake()
    {
        panel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (panelIsActive == false)
            {
                StartCoroutine(Show());
                panelIsActive = true;
            }
            else if (panelIsActive == true)
            {
                StartCoroutine(Hide());
                panelIsActive = false;
            }
        }
    }

    public IEnumerator Show()
    {
        panel.SetActive(true);
        playerController.canMove = false;
        yield return new WaitForSeconds(delay);
    }


    public IEnumerator Hide() 
    {
        panel.SetActive(false);
        playerController.canMove = true;
        yield return new WaitForSeconds(delay);
    }
}
