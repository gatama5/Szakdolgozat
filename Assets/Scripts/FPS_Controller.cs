
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

    public float gravity = 20f;

    CharacterController characterController;

    void Start()
    {
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

        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        RaycastHit hit;
        float rayDistance = 0.5f;
        int layerMask = ~LayerMask.GetMask("Ignore Raycast"); 

        if (curSpeedX > 0 && Physics.Raycast(transform.position, forward, out hit, rayDistance, layerMask))
        {
            if (hit.collider != null && !hit.collider.isTrigger)
            {
                curSpeedX = 0;
            }
        }

        if (curSpeedX < 0 && Physics.Raycast(transform.position, -forward, out hit, rayDistance, layerMask))
        {
            if (hit.collider != null && !hit.collider.isTrigger)
            {
                curSpeedX = 0;
            }
        }

        if (curSpeedY > 0 && Physics.Raycast(transform.position, right, out hit, rayDistance, layerMask))
        {
            if (hit.collider != null && !hit.collider.isTrigger)
            {
                curSpeedY = 0;
            }
        }

        if (curSpeedY < 0 && Physics.Raycast(transform.position, -right, out hit, rayDistance, layerMask))
        {
            if (hit.collider != null && !hit.collider.isTrigger)
            {
                curSpeedY = 0;
            }
        }

        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

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