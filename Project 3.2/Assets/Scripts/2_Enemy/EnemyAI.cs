using UnityEngine;

public struct EnemyState
{
    public EnemyAction CurrentAction;
    public EnemyAttack CurrentAttack;

    public Vector3 PlayerPosition;
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
    public bool isActive;
    public bool ShowDebug;

    #region * Variables
    [Header("State Machine Control")]
    [SerializeField] private float stateChangeCooldown = 5f;
    [SerializeField] [Range(0f, 100f)] private float attackChance = 50f;    // % chance that the state machine will choose to attack over movement
    private float _cooldownTimer;

    [Header("Attacks")]
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private Transform projectileSpawn;
    [SerializeField] private ProjectilePool projectilePoolA;
    [SerializeField] private ProjectilePool projectilePoolB;
    [SerializeField] private float playerDetectionRange;
    [SerializeField] private EnemyAttack[] attacks;

    [Header("Movement")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float movementRadius = 20f;

    // State Machine Control 
    private EnemyState _state;
    private EnemyState _prevState;

    // Unity Components
    private Rigidbody _rb;
    private Enemy _enemy;
    private EnemyAnimationController _animationController;

    // Misc.
    private bool _isActive; // <-------------------------------------- True/False if the state machine is active
    private readonly Collider[] _detectionHits = new Collider[10];  // OverlapSphereNonAlloc buffer for player detection
    #endregion


    #region * Debug Messages
    void OnDrawGizmosSelected()
    {
        if (!ShowDebug) return;
        Gizmos.color = Color.red;
        if (_state.MovementTarget != Vector3.zero) Gizmos.DrawSphere(_state.MovementTarget, 0.5f);
        Gizmos.color = Color.orange;
        Gizmos.DrawWireSphere(transform.position, playerDetectionRange);
    }
    void OnGUI()
    {
        if (!ShowDebug) return;
        var debugText = $"Current State: {_state.CurrentAction} ({(int)_state.CurrentAction})\n"
                        + $"Current Attack: {_state.CurrentAttack?.attackID ?? 0}\n"
                        + $"State Machine Cooldown: {_cooldownTimer:F2} sec\n"
                        + $"Player Position: {_state.PlayerPosition}\n";
        var altText = "EnemyAI Disabled";
        var result = _isActive ? debugText : altText;
        GUI.Label(new Rect(10, 10, 300, 100), result);
    }
    #endregion
    
    #region * Initialization
    // Called by 'Enemy.cs' in 'Start()' function
    public void Initialize(Enemy enemy, EnemyAnimationController controller)
    {
        _enemy = enemy;
        _rb = GetComponent<Rigidbody>();
        _animationController = controller;

        SetToIdle();

        TrackPlayer();
        _state.PlayerPosition = _state.PlayerPosition != null ? _state.PlayerPosition : Vector3.zero;

        _cooldownTimer = 0f;
        _isActive = true;
    }
    #endregion

    #region * Update Functions
    // Called by 'Enemy.cs' in 'Update()' function
    public void UpdateAI(float deltaTime)
    {
        _isActive = isActive;   // (delete later)

        // Track player position
        TrackPlayer();

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
    // Called by 'Enemy.cs' in 'LateUpdate()' function
    public void LateUpdateAI(float deltaTime)
    {
        // Perform attack
        if (_state.CurrentAction is EnemyAction.Attack) UpdateCurrentAttack();

        // Update Previous State
        _prevState = _state;
    }
    // Called by 'Enemy.cs' in 'FixedUpdate()' function
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
                _enemy.SetToIdle();
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

    #region * Movement
    // Movement START
    private void EnterMoveState(Vector3 position)
    {
        _state.CurrentAction = EnemyAction.Move;
        _state.MovementTarget = position;
    }
    #endregion

    #region * Attacks
    // Attack START
    private void EnterAttackState()
    {
        // Update State Machine
        _state.CurrentAction = EnemyAction.Attack;

        // Select an Attack to perform
        var rand = Random.Range(0, attacks.Length);
        var attack = attacks[rand];    // * placeholder attack selection *
        _state.CurrentAttack = attack;

        // Initialize Attack
        _state.CurrentAttack.Initialize();
    }

    // ATTACK
    private void UpdateCurrentAttack()
    {
        // Attack END
        if (!_state.CurrentAttack || _state.CurrentAttack.attackComplete)
        {
            _animationController.SetBool("AttackActive", false);
            SetToIdle();
            return;
        }

        // Attack IDLE
        if (_animationController.GetBool("AttackActive"))
        {
            // Adjust projectile spawn to be at player height
            Vector3 targetSpawn = projectileSpawn.position;
            targetSpawn.y = 0.5f;   // * magic number alert *
            projectileSpawn.position = targetSpawn;

            // Initialize Attack Context
            var context = new EnemyAttackContext
            {
                Enemy                   = gameObject.GetComponent<Enemy>(),
                AnimationController     = _animationController,
                ProjectilePool          = projectilePoolA,
                SecondaryProjectilePool = projectilePoolB,
                PlayerPosition          = _state.PlayerPosition,
                PlayerLayer             = targetLayer,
                HitboxSpawn             = projectileSpawn,
            };

            // Trigger Attack
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

    // Updates '_state.PlayerPosition' (if player in range)
    private void TrackPlayer()
    {
        var hits = Physics.OverlapSphereNonAlloc
        (
            transform.position,
            playerDetectionRange,
            _detectionHits,
            targetLayer
        );

        if (hits > 0)
        {
            var hit = _detectionHits[0];
            _state.PlayerPosition = Vector3.ProjectOnPlane(hit.gameObject.transform.position, Vector3.up);
        }
    }
    #endregion

    #region * Gateway Functions
    // Set movement target
    public void SetMovementTarget(Vector3 position) => _state.MovementTarget = position;

    // Set character rotation
    public void RotateTowards(Vector3 direction) => transform.rotation = Quaternion.LookRotation(direction);

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

    // Returns player position
    public Vector3 GetPlayerPosition() => _state.PlayerPosition;
    #endregion
}
