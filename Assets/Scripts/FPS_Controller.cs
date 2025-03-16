
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]

public class FPS_Controller : MonoBehaviour
{
    public Camera playerCamera;
    public float walkSpeed = 6f;
    public float runSpeed = 12f;

    public float lookSpeed = 2f;
    public float lookXLimit = 45f;

    Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;

    public bool canMove = true;

    // Gravit�ci� hozz�ad�sa
    public float gravity = 20f;

    // �tk�z�s ellen�rz�s
    private bool isColliding = false;

    CharacterController characterController;

    void Start()
    {
        //Debug.Log(Application.persistentDataPath); DB path
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        #region Handles Movment
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float curSpeedX = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Horizontal") : 0;

        // Gravit�ci� kezel�se
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        // �tk�z�svizsg�lat el�re �s oldal ir�nyokban
        RaycastHit hit;
        float rayDistance = 0.5f; // A sug�r t�vols�ga
        int layerMask = ~LayerMask.GetMask("Ignore Raycast"); // Minden layer kiv�ve az Ignore Raycast

        // El�re ir�ny� �tk�z�s ellen�rz�se
        if (curSpeedX > 0 && Physics.Raycast(transform.position, forward, out hit, rayDistance, layerMask))
        {
            // Ha nem trigger, akkor blokkoljuk a mozg�st
            if (hit.collider != null && !hit.collider.isTrigger)
            {
                curSpeedX = 0;
            }
        }

        // H�tra ir�ny� �tk�z�s ellen�rz�se
        if (curSpeedX < 0 && Physics.Raycast(transform.position, -forward, out hit, rayDistance, layerMask))
        {
            // Ha nem trigger, akkor blokkoljuk a mozg�st
            if (hit.collider != null && !hit.collider.isTrigger)
            {
                curSpeedX = 0;
            }
        }

        // Jobb oldali �tk�z�s ellen�rz�se
        if (curSpeedY > 0 && Physics.Raycast(transform.position, right, out hit, rayDistance, layerMask))
        {
            // Ha nem trigger, akkor blokkoljuk a mozg�st
            if (hit.collider != null && !hit.collider.isTrigger)
            {
                curSpeedY = 0;
            }
        }

        // Bal oldali �tk�z�s ellen�rz�se
        if (curSpeedY < 0 && Physics.Raycast(transform.position, -right, out hit, rayDistance, layerMask))
        {
            // Ha nem trigger, akkor blokkoljuk a mozg�st
            if (hit.collider != null && !hit.collider.isTrigger)
            {
                curSpeedY = 0;
            }
        }

        // Mozg�si ir�nyt friss�tj�k a potenci�lisan m�dos�tott sebess�gek alapj�n
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        // Gravit�ci� alkalmaz�sa
        if (characterController.isGrounded)
        {
            moveDirection.y = 0;
        }
        else
        {
            moveDirection.y = movementDirectionY - gravity * Time.deltaTime;
        }
        #endregion

        #region Handles Rotation
        // Mozg�s alkalmaz�sa a CharacterController-re
        characterController.Move(moveDirection * Time.deltaTime);

        if (canMove)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }
        #endregion

        #region Mouse Hide/Show
        if (Input.GetKey(KeyCode.H))
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else if (Input.GetKey(KeyCode.J))
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
        }
        #endregion
    }

    public void canMoveAgain()
    {
        canMove = true;
    }
}