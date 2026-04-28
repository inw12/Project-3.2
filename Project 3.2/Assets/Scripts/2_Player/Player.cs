using UnityEngine;
public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    public bool ShowDebug;

    [Header("Core Components")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerCombat playerCombat;

    [Header("Animation")]
    [SerializeField] private PlayerAnimationController animationController;
    [SerializeField] private PlayerAnimationRig animationRig;

    [Header("Misc")]
    [SerializeField] private CapsuleCollider hurtbox;
    [SerializeField] private CapsuleCollider parrybox;
    [SerializeField] private LayerMask groundLayer;

    // Player Input
    private PlayerInput _input;
    private bool _inputEnabled;

    private Vector3 _mousePosition;


    void OnGUI()
    {
        if (!ShowDebug) return;
        //GUILayout.Label($"Movement Action: {GetCurrentMovementAction()}");
        //GUILayout.Label($"Combat Action: {GetCurrentCombatAction()}");
        GUILayout.Label($"Movement Input Enabled: {playerMovement.MovementInputEnabled()}");
        GUILayout.Label($"Combat Input Enabled: {playerCombat.CombatInputEnabled()}");
        GUILayout.Label($"Combat Input Enabled: {playerCombat.ParryInputEnabled()}");
    }


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
        playerMovement.Initialize(hurtbox);
        playerCombat.Initialize(animationController, hurtbox);

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


    #region *--- Player Input Access --------------------------------------------------*
    // ALL Inputs
    public void InputEnabled(bool b) 
    {
        MovementInputEnabled(b);
        CombatInputEnabled(b);
    }
    // Movement Inputs
    public void MovementInputEnabled(bool b) => playerMovement.MovementInputEnabled(b);
    // Combat Inputs
    public void CombatInputEnabled(bool b) => playerCombat.CombatInputEnabled(b);
    // Parry Input
    public void ParryInputEnabled(bool b) => playerCombat.ParryInputEnabled(b);
    #endregion


    #region *--- Current Action Getters --------------------------------------------------*
    public MovementAction GetCurrentMovementAction() => playerMovement.GetState().CurrentAction;
    public CombatAction GetCurrentCombatAction() => playerCombat.GetState().CurrentAction;
    #endregion


    #region *--- 'PlayerMovement' Access ----------------------*
    // Set Velocity
    public void SetVelocity(Vector3 velocity, float acceleration) => playerMovement.SetVelocity(velocity, acceleration);
    // Set Rotation
    public void SetRotation(Quaternion rotation) => playerMovement.SetRotation(rotation);
    #endregion


    #region *--- 'PlayerCombat' Access ----------------------*
    // Toggle Melee Hitbox
    public void MeleeHitboxEnabled(bool b) => playerCombat.MeleeHitboxEnabled(b);
    // Combat Action Setter
    public void SetCurrentCombatAction(CombatAction action) => playerCombat.SetCurrentCombatAction(action);
    #endregion
}