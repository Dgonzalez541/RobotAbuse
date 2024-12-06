using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RobotAbuse
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Speed")]
        [SerializeField] public float MoveSpeed = 300f;

        [Header("Look Sensitivity")]
        [SerializeField] private float lookSensitivity = 1f;
        [SerializeField] private float upDownRange = 80f;

        private CharacterController characterController;
        private Camera mainCamera;

        PlayerInput playerInput;
        InputAction moveAction;
        InputAction lookAction;
        InputAction fireAction;

        MovementController movement;
        ObjectViewer objectViewer;

        private float verticalRotation;

        private void Awake()
        {
            characterController = gameObject.GetComponent<CharacterController>();
            mainCamera = Camera.main;

            playerInput = gameObject.GetComponent<PlayerInput>();
            moveAction = playerInput.actions["Move"];
            lookAction = playerInput.actions["Look"];
            fireAction = playerInput.actions["Fire"];

            fireAction.performed += OnFire;

            movement = new MovementController();
            objectViewer = new ObjectViewer();
        }

        private void OnFire(InputAction.CallbackContext context)
        {
            var ray = mainCamera.ScreenPointToRay(Mouse.current.position.value);
            objectViewer.DetectObject(ray);
        }

        Vector2 look;
        private void Update()
        {
            HandleMovement();
            HandleRotation();
        }

        private void HandleMovement()
        {
            var time = Time.deltaTime;
            var moveInput = moveAction.ReadValue<Vector3>();
            Vector3 input = movement.Calculate(transform, moveInput, MoveSpeed, time);

            characterController.Move(input);
        }

        private void HandleRotation()
        {
            var lookInput = lookAction.ReadValue<Vector2>();
            transform.Rotate(0, lookInput.x, 0); //Horizontal Rotation

            verticalRotation = movement.CalculateVerticalRotation(lookInput, verticalRotation, lookSensitivity, upDownRange);
            mainCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0); 
        }
    }
}
