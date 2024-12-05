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
        [SerializeField] private float lookSensitivity = 2f;
        [SerializeField] private float upDownRange = 80f;

        private CharacterController characterController;
        private Camera mainCamera;

        PlayerInput playerInput;
        InputAction moveAction;
        InputAction lookAction;

        Movement movement;

        private float verticalRotation;

        private void Awake()
        {
            characterController = gameObject.GetComponent<CharacterController>();
            mainCamera = Camera.main;

            playerInput = gameObject.GetComponent<PlayerInput>();
            moveAction = playerInput.actions["Move"];
            lookAction = playerInput.actions["Look"];

            movement = new Movement();
        }

        Vector2 look;
        private void Update()
        {
            HandleMovement();
            HandleRotation();
            HandleFire();
        }

        private void HandleFire()
        {
            throw new NotImplementedException();
        }

        private void HandleMovement()
        {
            var time = Time.deltaTime;
            var moveInput = moveAction.ReadValue<Vector3>();

            Vector3 input = movement.Calculate(transform, moveInput, MoveSpeed, time);

            characterController.Move(input);
        }

        /*private Vector3 CalculateMovement(float time, Vector3 moveInput, float speed)
        {
            var input = new Vector3();
            input += transform.forward * moveInput.y;
            input += transform.right * moveInput.x;
            input += transform.up * moveInput.z;
            input = Vector3.ClampMagnitude(input, 1f);
            input = input * speed * time;
            return input;
        }*/

        private void HandleRotation()
        {
            var lookInput = lookAction.ReadValue<Vector2>();
            float mouseXRotation = lookInput.x;
            transform.Rotate(0, mouseXRotation, 0);

            verticalRotation -= lookInput.y * lookSensitivity;
            verticalRotation = Mathf.Clamp(verticalRotation, -upDownRange, upDownRange);
            mainCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
        }
    }
}
