using UnityEngine;
public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    [Header("Core Components")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerCombat playerCombat;

    [Header("Animation")]
    [SerializeField] private PlayerAnimationController animationController;
    [SerializeField] private PlayerAnimationRig animationRig;

    [Header("Misc")]
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

        // Core Components
        playerMovement.Initialize();
        playerCombat.Initialize();

        // Animation
        animationController.Initialize();
        animationRig.Initialize();
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
            Roll            = _inputEnabled && input.Roll.WasPressedThisFrame(),
            MousePosition   = _inputEnabled ? _mousePosition : Vector3.zero
        };
        playerMovement.UpdateInput(movementInput);

        // Combat Input
        var combatInput = new CombatInput
        {
            Ranged          = _inputEnabled && input.Mouse1.IsPressed(),
            Melee           = _inputEnabled && input.Mouse2.WasPressedThisFrame(),
            Parry           = _inputEnabled && input.Parry.WasPressedThisFrame(),
            MousePosition   = _inputEnabled ? _mousePosition : Vector3.zero
        };
        playerCombat.UpdateInput(combatInput);
    }

    /// <summary>
    /// * Updates...
    ///     - Player Rotation
    ///     - Player Combat Action
    ///     - Animation Controller
    ///     - Animation Rig
    /// </summary>
    void LateUpdate()
    {
        var deltaTime = Time.deltaTime;

        // Update character rotation
        playerMovement.UpdateRotation(deltaTime);

        // Update combat action
        playerCombat.UpdateCombatAction(deltaTime);

        // Update Animations
        animationController.UpdateAnimator();
        animationRig.UpdateRig();
    }

    // Updates Player Movement
    void FixedUpdate()
    {
        var fixedDeltaTime = Time.fixedDeltaTime;
        playerMovement.UpdateMovement(fixedDeltaTime);
    }

    void OnDisable() => _input.Dispose();


    #region *--- Public Getters --------------------------------------------------*
    public MovementAction GetCurrentMovementAction() => playerMovement.GetState().CurrentAction;
    public CombatAction GetCurrentCombatAction() => playerCombat.GetState().CurrentAction;
    #endregion


    #region *--- Public Methods for Player Component Access ----------------------*
    // Toggle Movement Input
    public void MovementInputEnabled(bool b) => playerMovement.MovementInputEnabled(b);
    // Toggle Combat Input 
    public void CombatInputEnabled(bool b) => playerCombat.CombatInputEnabled(b);
    // Toggle Melee Hitbox
    public void MeleeHitboxEnabled(bool b) => playerCombat.MeleeHitboxEnabled(b);
    #endregion
}