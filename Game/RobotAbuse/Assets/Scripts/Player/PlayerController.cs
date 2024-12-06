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
        InputAction lookTriggerAction;

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
            lookTriggerAction = playerInput.actions["LookTrigger"];

            fireAction.performed += OnFire;

            lookTriggerAction.started += OnLookTrigger;
            lookTriggerAction.canceled += OnLookTriggerCancled;

            movement = new MovementController();

            objectViewer = new ObjectViewer();
        }
        private void Update()
        {
            if (movement.IsMoving)
            {
                HandleMovement();
                HandleRotation();
            }
        }

        private void OnLookTriggerCancled(InputAction.CallbackContext context)
        {
            movement.IsMoving = false;
        }

        private void OnLookTrigger(InputAction.CallbackContext context)
        {
            movement.IsMoving = true;
        }

        private void OnFire(InputAction.CallbackContext context)
        {
            var ray = mainCamera.ScreenPointToRay(Mouse.current.position.value);
            objectViewer.DetectObject(ray);
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
