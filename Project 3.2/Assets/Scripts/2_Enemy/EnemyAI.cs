using UnityEngine;

public struct EnemyState
{
    public EnemyAction CurrentAction;
    public EnemyAttack CurrentAttack;

    public Vector3 MovementTarget;
}
public enum EnemyAction
{
    Idle            = 0,
    Move            = 1,
    Attack          = 2
}

[RequireComponent(typeof(Rigidbody))]
public class EnemyAI : MonoBehaviour
{
    public bool ShowDebug;

    #region * Variables
    [Header("State Machine Control")]
    [SerializeField] private float stateChangeCooldown = 5f;
    [SerializeField] [Range(0f, 100f)] private float attackChance = 50f;    // % chance that the state machine will choose to attack over movement
    private float _cooldownTimer;

    [Header("Attacks")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private Transform projectileSpawn;
    [SerializeField] private ProjectilePool projectilePool;
    [SerializeField] private EnemyAttack[] attacks;

    [Header("Movement")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float movementRadius = 20f;

    // State Machine Control 
    private EnemyState _state;
    private EnemyState _prevState;

    // Unity Components
    private Rigidbody _rb;
    private EnemyAnimationController _animationController;

    // Misc. Variables
    private bool _isActive;         // true/false if the state machine is active
    #endregion


    #region * Debug Messages
    void OnDrawGizmosSelected()
    {
        if (!ShowDebug) return;
        Gizmos.color = Color.red;
        if (_state.MovementTarget != Vector3.zero) Gizmos.DrawSphere(_state.MovementTarget, 0.5f);
    }
    void OnGUI()
    {
        if (!ShowDebug) return;
        var debugText = $"Current State: {_state.CurrentAction} ({(int)_state.CurrentAction})\n"
                        + $"Current Attack: {_state.CurrentAttack?.attackID ?? 0}\n"
                        + $"State Machine Cooldown: {_cooldownTimer:F2} sec\n";
        GUI.Label(new Rect(10, 10, 300, 100), debugText);
    }
    #endregion
    

    #region * Initialization
    // Start()
    public void Initialize(EnemyAnimationController controller)
    {
        _rb = GetComponent<Rigidbody>();
        _animationController = controller;

        SetToIdle();

        _cooldownTimer = 0f;
        _isActive = true;
    }
    #endregion


    #region * Update
    // Update()
    public void UpdateAI(float deltaTime)
    {
        // Only update State Machine if active
        if (_isActive)
        {
            // Update cooldown timer when Idle
            if (_state.CurrentAction is EnemyAction.Idle) _cooldownTimer += deltaTime;

            // State Machine Control
            if (_cooldownTimer >= stateChangeCooldown)
            {
                // Choose a random action
                var rand = Random.Range(0f, 100f);
                // Attack
                if (rand <= attackChance)
                {
                    EnterAttackState();
                }
                // Movement
                else
                {
                    EnterMoveState(GetRandomPosition(movementRadius));
                }

                _cooldownTimer = 0f;
            }
        }
    }
    // LateUpdate()
    public void LateUpdateAI(float deltaTime)
    {
        // Perform attack
        if (_state.CurrentAction is EnemyAction.Attack) UpdateCurrentAttack();

        // Update Previous State
        _prevState = _state;
    }
    // FixedUpdate()
    public void UpdateMovement(float fixedDeltaTime)
    {
        /// * Enemy can only move when...
        ///     - in "Move" state
        ///     - performing an attack that requires movement
        if (_state.CurrentAction is EnemyAction.Move || (_state.CurrentAttack && _state.CurrentAttack.requiresMovement))
        {
            // If destination reached, EXIT movement state
            if (_rb.position == _state.MovementTarget)
            {
                _animationController.SetBool("AttackActive", false);
                SetToIdle();
                return;
            }

            // Rotate towards movement target
            var direction = (_state.MovementTarget - Vector3.ProjectOnPlane(transform.position, Vector3.up)).normalized;
            transform.rotation = Quaternion.LookRotation(direction);

            // Calculate amount to move this frame
            var next = Vector3.MoveTowards
            (
                _rb.position,
                _state.MovementTarget,
                1f - Mathf.Exp(-speed * fixedDeltaTime)
            );

            // Apply movement
            _rb.MovePosition(next);
        }
    }
    #endregion


    #region * State Machine Actions
    // Changes state to "Move" & updates target movement position
    private void EnterMoveState(Vector3 position)
    {
        _state.CurrentAction = EnemyAction.Move;
        _state.MovementTarget = position;
    }
    // Changes state to "Attack" and selects an attack to perform from list
    private void EnterAttackState()
    {
        // Update State Machine
        _state.CurrentAction = EnemyAction.Attack;

        // Select an Attack to perform
        var attack = attacks[0];    // * placeholder attack selection *
        _state.CurrentAttack = attack;

        // Initialize Attack
        _state.CurrentAttack.Initialize();
    }
    // Calls the 'Attack' function of the current attack (every frame)
    private void UpdateCurrentAttack()
    {
        // Return to Idle if 'CurrentAttack' is null OR if 'CurrentAttack' is complete
        if (!_state.CurrentAttack || _state.CurrentAttack.attackComplete)
        {
            _animationController.SetBool("AttackActive", false);
            SetToIdle();
            return;
        }

        if (_animationController.GetBool("AttackActive"))
        {
            var context = new EnemyAttackContext
            {
                Enemy           = gameObject.GetComponent<Enemy>(),
                HitboxSpawn     = projectileSpawn,
                ProjectilePool  = projectilePool,
                PlayerLayer     = playerLayer
            };
            _state.CurrentAttack.Attack(context);
        }
    }
    #endregion


    #region * Helper Functions 
    // Returns a random Vector3 position within given radius (projected onto y-axis)
    private Vector3 GetRandomPosition(float radius)
    {
        var target = Random.insideUnitSphere * radius;
        target = Vector3.ProjectOnPlane(target, Vector3.up);
        return target;
    }
    #endregion


    #region * Public Access 
    // State Getters
    public EnemyState GetState() => _state;
    public EnemyState GetPrevState() => _prevState;
    // Enable/Disable Enemy AI
    public void EnemyActive(bool b) => _isActive = b;
    // Reset Enemy AI
    public void SetToIdle()
    {
        _state.CurrentAction = EnemyAction.Idle;
        _state.CurrentAttack = null;
        _state.MovementTarget = Vector3.zero;

        _cooldownTimer = 0f;
    }
    // Set movement target
    public void SetMovementTarget(Vector3 position)
    {
        _state.MovementTarget = position;
    }
    #endregion
}
