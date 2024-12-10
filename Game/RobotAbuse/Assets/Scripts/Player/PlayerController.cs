using UnityEngine;
using UnityEngine.InputSystem;

namespace RobotAbuse
{
    //The PlayerController exists to take in input from the Input System and pass it to the appropriate systems, in this case, the ObjectViewer and the MovementController.
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Speed")]
        [SerializeField] public float MoveSpeed = 300f;

        [Header("Look Sensitivity")]
        [SerializeField] private float lookSensitivity = 1f;
        [SerializeField] private float upDownRange = 80f;

        private CharacterController characterController;
        private Camera mainCamera;

        //Input System
        PlayerInput playerInput;
        InputAction moveAction;
        InputAction lookAction;
        InputAction fireAction;
        InputAction lookTriggerAction;

        //Behaviors 
        MovementController movement;
        ObjectViewer objectViewer;

        //Value for the vertical rotation of the camera when looking
        private float verticalCameraRotation;

        //Value for the mouse position when dragging objects.
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
            HandleMovement();
            HandleRotation();
            HandleDragging();
            HandleObjectSensing();
        }

        private void HandleMovement()
        {
            if (movement.IsMoving)
            {
                var time = Time.deltaTime;
                var moveInput = moveAction.ReadValue<Vector3>();
                Vector3 input = movement.Calculate(transform, moveInput, MoveSpeed, time);

                characterController.Move(input);
            }
        }

        private void HandleRotation()
        {
            if (movement.IsMoving)
            {
                var lookInput = lookAction.ReadValue<Vector2>();
                transform.Rotate(0, lookInput.x, 0); //Horizontal Rotation

                verticalCameraRotation = movement.CalculateVerticalRotation(lookInput, verticalCameraRotation, lookSensitivity, upDownRange);
                mainCamera.transform.localRotation = Quaternion.Euler(verticalCameraRotation, 0, 0);
            }
        }

        private void HandleDragging()
        {
            if (objectViewer.IsDragging)
            {
                var inputPosition = Mouse.current.position.value;
                var pos = mainCamera.ScreenToWorldPoint(new Vector3(inputPosition.x, inputPosition.y, 0) - mousePosition);
                objectViewer.DragObject(pos);
            }
        }

        private void HandleObjectSensing()
        {
            if (!objectViewer.IsDragging)
            {
                var ray = mainCamera.ScreenPointToRay(Mouse.current.position.value);
                objectViewer.DetectObject(ray);
            }
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

        }
}
