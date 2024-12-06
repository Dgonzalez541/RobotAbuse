using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
   [Header("Input Action Asset")]
   [SerializeField] private InputActionAsset playerControls;

    [Header("Action Map Name References")]
    [SerializeField] private string actionMapName = "Player";

    [Header("Acton Name References")]
    [SerializeField] private string moveActionName = "Move";
    [SerializeField] private string ascendActionName = "Ascend";
    [SerializeField] private string lookActionName = "Look";
    [SerializeField] private string fireActionName = "Fire";

    private InputAction moveAction;
    private InputAction ascendAction;
    private InputAction lookAction;
    private InputAction fireAction;

    public Vector2 MoveInput { get; private set; }
    public Vector2 AscendInput { get; set; }
    public Vector2 lookInput { get; private set; }
    public bool FireInput { get; private set; }

    public static InputHandler Instance { get; private set; }

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        moveAction = playerControls.FindActionMap(actionMapName).FindAction(moveActionName);
        ascendAction = playerControls.FindActionMap(actionMapName).FindAction(ascendActionName);
        lookAction = playerControls.FindActionMap(actionMapName).FindAction(lookActionName);
        fireAction = playerControls.FindActionMap(actionMapName).FindAction(fireActionName);

        moveAction.performed += context => MoveInput = context.ReadValue<Vector2>();
        moveAction.canceled += context => MoveInput = Vector2.zero;

        ascendAction.performed += context => AscendInput = context.ReadValue<Vector2>();
        ascendAction.canceled += context => AscendInput = Vector2.zero;

        lookAction.performed += context => lookInput = context.ReadValue<Vector2>();
        lookAction.canceled += context => lookInput = Vector2.zero;

        fireAction.performed += context => FireInput = true;
        fireAction.canceled += context => FireInput = false;
    }

    private void OnEnable()
    {
        moveAction.Enable();
        ascendAction.Enable();
        lookAction.Enable();
        fireAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        ascendAction.Disable();
        lookAction.Disable();
        fireAction.Disable();
    }

}
