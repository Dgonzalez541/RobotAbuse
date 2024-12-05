using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Speed")]
    [SerializeField] public float MoveSpeed = 300f;

    [Header("Look Sensitivity")]
    [SerializeField] private float lookSensitivity = 2f;
    [SerializeField] private float upDownRange = 80f;

    
    private CharacterController characterController;
    private Camera mainCamera;

    PlayerInput playerInput;
    InputAction moveAction;
    InputAction lookAction;
    InputAction ascendAction;

    private float verticalRotation;

    private void Awake()
    {
        characterController = gameObject.GetComponent<CharacterController>();
        mainCamera = Camera.main;

        playerInput = gameObject.GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
        lookAction = playerInput.actions["Look"];
    }

    Vector2 look;
    private void Update()
    {
        HandleMovement();
        HandleRotation();
        //HandleFire();
    }

    private void HandleFire()
    {
        throw new NotImplementedException();
    }

    private Vector3 currentMovement;
    private void HandleMovement()
    {
        var moveInput = moveAction.ReadValue<Vector3>();
        var input = new Vector3();
        input += transform.forward * moveInput.y;
        input += transform.right * moveInput.x;
        input += transform.up * moveInput.z;
        input = Vector3.ClampMagnitude(input, 1f);
        characterController.Move(input * MoveSpeed *  Time.deltaTime);
    }

    private void HandleRotation()
    {
        var lookInput = lookAction.ReadValue<Vector2>();
        float mouseXRotation = lookInput.x;
        transform.Rotate(0,mouseXRotation,0);

        verticalRotation -= lookInput.y * lookSensitivity;
        verticalRotation = Mathf.Clamp(verticalRotation, -upDownRange, upDownRange);
        mainCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }
}
