using UnityEngine;
using UnityEngine.InputSystem;

namespace RobotAbuse
{
    //PlayerController takes in inputs from the Input System and passes them along to behaviours such as movement and Object viewing.
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

        //Behaviours
        MovementController movement;
        ObjectViewer objectViewer;

        
        private float verticalRotation;//Used for camera looking.
        private Vector3 mousePosition;//Used for object dragging.

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

        Vector3 currentInputVector;
        private Vector3 smoothInputVelocity;

        private void HandleMovement()
        {
            if (movement.IsMoving)
            {
                var moveInput = moveAction.ReadValue<Vector3>();
                var moveVector = movement.Calculate(transform, moveInput, MoveSpeed);
                characterController.Move(moveVector * Time.deltaTime);
                Debug.Log(moveVector);
            }
        }

        private void HandleRotation()
        {
            if (movement.IsMoving)
            {
                var lookInput = lookAction.ReadValue<Vector2>();
                transform.Rotate(0, lookInput.x, 0); //Horizontal Rotation

                verticalRotation = movement.CalculateVerticalRotation(lookInput, verticalRotation, lookSensitivity, upDownRange);
                mainCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
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
            if(objectViewer.DetectedGameObject != null && !objectViewer.IsDragging) 
            {
                objectViewer.OnObjectSelection();

                if(objectViewer.IsDragging)
                {
                    mousePosition = new Vector3(inputPosition.x, inputPosition.y, 0) - mainCamera.WorldToScreenPoint(objectViewer.DetectedGameObject.transform.position);
                }
            }
            
        }
        private void OnFireCancled(InputAction.CallbackContext context)
        {
            objectViewer.StopDragging();
        }
    }
}
