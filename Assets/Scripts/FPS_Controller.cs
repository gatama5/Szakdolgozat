
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

    // Gravitáció hozzáadása
    public float gravity = 20f;

    // Ütközés ellenõrzés
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

        // Gravitáció kezelése
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        // Ütközésvizsgálat elõre és oldal irányokban
        RaycastHit hit;
        float rayDistance = 0.5f; // A sugár távolsága
        int layerMask = ~LayerMask.GetMask("Ignore Raycast"); // Minden layer kivéve az Ignore Raycast

        // Elõre irányú ütközés ellenõrzése
        if (curSpeedX > 0 && Physics.Raycast(transform.position, forward, out hit, rayDistance, layerMask))
        {
            // Ha nem trigger, akkor blokkoljuk a mozgást
            if (hit.collider != null && !hit.collider.isTrigger)
            {
                curSpeedX = 0;
            }
        }

        // Hátra irányú ütközés ellenõrzése
        if (curSpeedX < 0 && Physics.Raycast(transform.position, -forward, out hit, rayDistance, layerMask))
        {
            // Ha nem trigger, akkor blokkoljuk a mozgást
            if (hit.collider != null && !hit.collider.isTrigger)
            {
                curSpeedX = 0;
            }
        }

        // Jobb oldali ütközés ellenõrzése
        if (curSpeedY > 0 && Physics.Raycast(transform.position, right, out hit, rayDistance, layerMask))
        {
            // Ha nem trigger, akkor blokkoljuk a mozgást
            if (hit.collider != null && !hit.collider.isTrigger)
            {
                curSpeedY = 0;
            }
        }

        // Bal oldali ütközés ellenõrzése
        if (curSpeedY < 0 && Physics.Raycast(transform.position, -right, out hit, rayDistance, layerMask))
        {
            // Ha nem trigger, akkor blokkoljuk a mozgást
            if (hit.collider != null && !hit.collider.isTrigger)
            {
                curSpeedY = 0;
            }
        }

        // Mozgási irányt frissítjük a potenciálisan módosított sebességek alapján
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        // Gravitáció alkalmazása
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
        // Mozgás alkalmazása a CharacterController-re
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