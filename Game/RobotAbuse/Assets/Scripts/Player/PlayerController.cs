using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RobotAbuse
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Speed")]
        [SerializeField] public float MoveSpeed = 1f;

        [Header("Look Sensitivity")]
        [SerializeField] private float lookSensitivity = .1f;
        [SerializeField] private float upDownRange = 80f;

        private CharacterController characterController;
        private Camera mainCamera;

        PlayerInput playerInput;
        InputAction moveAction;
        InputAction lookAction;
        InputAction fireAction;
        InputAction lookTriggerAction;

        MovementController movement;

        ObjectViewer objectViewer;

        private float verticalRotation;
        private Vector3 mousePosition;

        private void Awake()
        {
            characterController = gameObject.GetComponent<CharacterController>();
            mainCamera = Camera.main;

            playerInput = gameObject.GetComponent<PlayerInput>();
            moveAction = playerInput.actions["Move"];
            lookAction = playerInput.actions["Look"];
            fireAction = playerInput.actions["Fire"];
            lookTriggerAction = playerInput.actions["LookTrigger"];

            fireAction.performed += OnFire;
            fireAction.canceled += OnFireCancled;

            lookTriggerAction.started += OnLookTrigger;
            lookTriggerAction.canceled += OnLookTriggerCancled;

            movement = new MovementController();

            objectViewer = gameObject.GetComponent<ObjectViewer>();
        }

        private void Update()
        {
            if (movement.IsMoving)
            {
                HandleMovement();
                HandleRotation();
            }

            if(objectViewer.IsDragging)
            {
                HandleDragging();
            }

            if(!objectViewer.IsDragging)
            {
                HandleObjectSensing();
            }
            
        }

        private void HandleObjectSensing()
        {
            var ray = mainCamera.ScreenPointToRay(Mouse.current.position.value);
            objectViewer.DetectObject(ray);
        }

        private void HandleDragging()
        {
            var inputPosition = Mouse.current.position.value;
            var pos = mainCamera.ScreenToWorldPoint(new Vector3(inputPosition.x, inputPosition.y, 0) - mousePosition);
            objectViewer.DragObject(pos);
        }

        private void OnLookTrigger(InputAction.CallbackContext context)
        {
            movement.IsMoving = true;
        }

        private void OnLookTriggerCancled(InputAction.CallbackContext context)
        {
            movement.IsMoving = false;
        }

        private void OnFire(InputAction.CallbackContext context)
        {
            var inputPosition = Mouse.current.position.value;
            if(objectViewer.DetectedGameObject != null) 
            {
                mousePosition = new Vector3(inputPosition.x, inputPosition.y, 0) - mainCamera.WorldToScreenPoint(objectViewer.DetectedGameObject.transform.position);
                objectViewer.OnObjectDetected();
            }
            
        }
        private void OnFireCancled(InputAction.CallbackContext context)
        {
            objectViewer.StopDragging();
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
