using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RobotAbuse
{
    [RequireComponent(typeof(ObjectViewer))]
    //PlayerController takes in inputs from the Input System and passes them along to behaviours such as movement and Object viewing.
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Speed")]
        [SerializeField] public float MoveSpeed = 1f;

        [Header("Look Sensitivity")]
        [SerializeField] float lookSensitivity = .1f;
        [SerializeField] float upDownRange = 80f;

        CharacterController characterController;
        Camera mainCamera;

        PlayerInput playerInput;
        InputAction moveAction;
        InputAction lookAction;
        InputAction fireAction;
        InputAction lookTriggerAction;

        //Behaviours
        MovementController movement;
        ObjectViewer objectViewer;
        
        float verticalRotation;//Used for camera looking.
        Vector3 mousePosition;//Used for object dragging.

        public event EventHandler OnFireCanceledEvent;

        void Awake()
        {
            characterController = gameObject.GetComponent<CharacterController>();
            mainCamera = Camera.main;

            playerInput = gameObject.GetComponent<PlayerInput>();
            moveAction = playerInput.actions["Move"];
            lookAction = playerInput.actions["Look"];
            fireAction = playerInput.actions["Fire"];
            lookTriggerAction = playerInput.actions["LookTrigger"];

            fireAction.performed += OnFire;
            fireAction.canceled += OnFireCanceled;

            lookTriggerAction.started += OnLookTrigger;
            lookTriggerAction.canceled += OnLookTriggerCancled;

            movement = new MovementController();

            objectViewer = gameObject.GetComponent<ObjectViewer>();
        }

        void Update()
        {  
            HandleMovement();
            HandleRotation();     
            HandleDragging();
            HandleObjectSensing();
        }

        Vector3 currentInputVector;
        Vector3 smoothInputVelocity;

        void HandleMovement()
        {
            if (movement.IsMoving)
            {
                var moveInput = moveAction.ReadValue<Vector3>();
                var moveVector = movement.Calculate(transform, moveInput, MoveSpeed);
                characterController.Move(moveVector * Time.deltaTime);
            }
        }

        void HandleRotation()
        {
            if (movement.IsMoving)
            {
                var lookInput = lookAction.ReadValue<Vector2>();
                transform.Rotate(0, lookInput.x, 0); //Horizontal Rotation

                verticalRotation = movement.CalculateVerticalRotation(lookInput, verticalRotation, lookSensitivity, upDownRange);
                mainCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
            }
        }

        void HandleDragging()
        {
            if (objectViewer.IsDragging)
            {
                var inputPosition = Mouse.current.position.value;
                var pos = mainCamera.ScreenToWorldPoint(new Vector3(inputPosition.x, inputPosition.y, 0) - mousePosition);
                objectViewer.DragObject(pos);
            }
        }

        void HandleObjectSensing()
        {
            if (!objectViewer.IsDragging)
            {
                var ray = mainCamera.ScreenPointToRay(Mouse.current.position.value);
                objectViewer.DetectObject(ray);
            }
        }

        void OnLookTrigger(InputAction.CallbackContext context)
        {
            movement.IsMoving = true;
        }

        void OnLookTriggerCancled(InputAction.CallbackContext context)
        {
            movement.IsMoving = false;
        }

        void OnFire(InputAction.CallbackContext context)
        {
            var inputPosition = Mouse.current.position.value;
            if(objectViewer.SelectedViewableObject != null && !objectViewer.IsDragging) 
            {
                objectViewer.OnObjectSelection();

                if(objectViewer.IsDragging)
                {
                    mousePosition = new Vector3(inputPosition.x, inputPosition.y, 0) - mainCamera.WorldToScreenPoint(objectViewer.SelectedGameObject.transform.position);
                }
            }
        }
        void OnFireCanceled(InputAction.CallbackContext context)
        {
            OnFireCanceledEvent?.Invoke(this, EventArgs.Empty);
        }
    }
}
