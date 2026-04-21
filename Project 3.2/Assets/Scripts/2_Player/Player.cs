using UnityEngine;
public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerAnimationController animationController;
    [Space]
    [SerializeField] private LayerMask groundLayer;

    // Player Input
    private PlayerInput _input;
    private bool _inputEnabled;

    private Vector3 _mousePosition;

    // Singleton Initialization
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Component Initialization
    void Start()
    {
        // Player Input 
        _input = new PlayerInput();
        _input.Enable();
        _inputEnabled = true;

        // Player Components
        playerMovement.Initialize();
        animationController.Initialize();
    }

    void Update()
    {
        // Record Mouse Position in World Space
        Ray cursorPosition = Camera.main.ScreenPointToRay(_input.General.Mouse.ReadValue<Vector2>());
        if (Physics.Raycast(cursorPosition, out RaycastHit hit, Mathf.Infinity, groundLayer)) {
            _mousePosition = hit.point;
        }

        // Movement Input
        var input = _input.General;
        var movementInput = new MovementInput
        {
            Movement        = _inputEnabled ? input.Move.ReadValue<Vector2>() : Vector2.zero,
            Roll           = _inputEnabled && input.Roll.WasPressedThisFrame(),
            MousePosition   = _inputEnabled ? _mousePosition : Vector3.zero
        };
        playerMovement.UpdateInput(movementInput);
    }

    /// <summary>
    /// * Updates...
    ///     - Player Rotation
    ///     - Animation Controller
    /// </summary>
    void LateUpdate()
    {
        var deltaTime = Time.deltaTime;

        playerMovement.UpdateRotation(deltaTime);
        animationController.UpdateAnimator();
    }

    // Updates Player Movement
    void FixedUpdate()
    {
        var fixedDeltaTime = Time.fixedDeltaTime;
        playerMovement.UpdateMovement(fixedDeltaTime);
    }

    void OnDisable() => _input.Dispose();
}
