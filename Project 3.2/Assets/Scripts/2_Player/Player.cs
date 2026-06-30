using UnityEngine;
public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    #region * Variables --------------------------------------------------
    [Header("Debugging Settings")]
    public bool ShowDebug;
    public Vector2 DebugPosition;

    [Header("Core Components")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerCombat playerCombat;

    [Header("Animation")]
    [SerializeField] private PlayerAnimationController animationController;
    [SerializeField] private PlayerAnimationRig animationRig;

    [Header("Health/Hurtbox")]
    [SerializeField] private PlayerHurtbox hurtbox; // basically a HP component

    [Header("Misc")]
    [SerializeField] private CapsuleCollider hurtboxCollider;
    [SerializeField] private CapsuleCollider parrybox;
    [SerializeField] private LayerMask groundLayer;
    [Space]
    [SerializeField] private GameObject weaponModel;

    // Player Input
    private PlayerInput _input;
    private bool _inputEnabled;

    private Vector3 _mousePosition;
    #endregion


    #region * Debug --------------------------------------------------
    void OnGUI()
    {
        if (!ShowDebug) return;

        var moveState = playerMovement.GetState();
        var combatState = playerCombat.GetState();

        var debugMessage =    $"HP: {hurtbox.GetHealthStatus().CurrentHealth} / {hurtbox.GetHealthStatus().MaxHealth}\n\n"
                            + $"Movement: {moveState.CurrentAction}\n"
                            + $"Velocity: {moveState.Velocity}\n\n"
                            + $"Combat: {combatState.CurrentAction}\n\n"
                            + $"Movement Input Enabled: {playerMovement.MovementInputEnabled()}\n"
                            + $"Combat Input Enabled: {playerCombat.CombatInputEnabled()}\n"
                            + $"Parry Input Enabled: {playerCombat.ParryInputEnabled()}";
        GUI.Label(new Rect(DebugPosition.x, DebugPosition.y, 500, 250), debugMessage);
    }
    #endregion



    #region * Initialization
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
    void Start()
    {
        // Player Input 
        _input = new PlayerInput();
        _input.Enable();
        _inputEnabled = true;

        // Core Components
        playerMovement.Initialize(hurtboxCollider);
        playerCombat.Initialize(animationController, hurtboxCollider);

        // Animation
        animationController.Initialize();
        animationRig.Initialize();

        // Hurtbox
        hurtbox.Initialize(animationController);
    }
    void OnDisable() => _input.Dispose();
    #endregion



    #region * Player Input
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
    #endregion



    #region * LateUpdate() --------------------------------------------------
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
    #endregion



    #region * FixedUpdate() --------------------------------------------------
    void FixedUpdate()
    {
        var fixedDeltaTime = Time.fixedDeltaTime;
        playerMovement.UpdateMovement(fixedDeltaTime);
    }
    #endregion



    #region * Input Gateway --------------------------------------------------
    // ALL Inputs
    public void InputEnabled(bool b) 
    {
        MovementInputEnabled(b);
        CombatInputEnabled(b);
        ParryInputEnabled(b);
    }
    
    // Movement Inputs
    public void MovementInputEnabled(bool b) => playerMovement.MovementInputEnabled(b);
    
    // Combat Inputs
    public void CombatInputEnabled(bool b) => playerCombat.CombatInputEnabled(b);
    
    // Parry Input
    public void ParryInputEnabled(bool b) => playerCombat.ParryInputEnabled(b);
    #endregion



    #region * Current Action Getters --------------------------------------------------
    public MovementAction GetCurrentMovementAction() => playerMovement.GetState().CurrentAction;
    public CombatAction GetCurrentCombatAction() => playerCombat.GetState().CurrentAction;
    #endregion



    #region * 'PlayerMovement' Gateway --------------------------------------------------
    // Set Velocity
    public void SetVelocity(Vector3 velocity, float acceleration) => playerMovement.SetVelocity(velocity, acceleration);

    // Set Rotation
    public void SetRotation(Quaternion rotation) => playerMovement.SetRotation(rotation);
    
    // CharacterController Toggle
    public void CharacterControllerEnabled(bool b) => playerMovement.CharacterControllerEnabled(b);
    
    // Exit Movement State
    public void ExitMovementState() => playerMovement.ExitMovementState();
    #endregion



    #region * 'PlayerCombat' Gateway --------------------------------------------------
    // Combat Action Setter
    public void SetCurrentCombatAction(CombatAction action) => playerCombat.SetCurrentCombatAction(action);
    // Exit Combat State
    public void ExitCombatState() => playerCombat.ExitCombatState();
    #endregion



    #region * Animation Controller Gateway -------------------------------------------------- 
    public void SetBoolean(string s, bool b) => animationController.SetBoolean(s, b);
    #endregion

    public void SetToIdle() 
    {
        playerMovement.ExitMovementState();
        playerCombat.ExitCombatState();
        animationController.UpdateAnimator();
        animationController.SetToIdle();
    }

    public void EnterParryPhase()
    {
        SetToIdle();
        CharacterControllerEnabled(false);
        InputEnabled(false);
        ParryInputEnabled(true);
        SetBoolean("InParryPhase", true);
        animationController.ApplyRootMotion(false);
    }
    public void ExitParryPhase()
    {
        SetToIdle();
        CharacterControllerEnabled(true);
        InputEnabled(true);
        SetBoolean("InParryPhase", false);
        animationController.ApplyRootMotion(true);
    }
}