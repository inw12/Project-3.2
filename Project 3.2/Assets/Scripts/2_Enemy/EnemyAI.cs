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

    // Misc. Variables
    private bool _isActive;
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
    public void Initialize()
    {
        _rb = GetComponent<Rigidbody>();

        SetToIdle();
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
                // Random Movement
                MoveTo(GetRandomPosition(movementRadius));

                _cooldownTimer = 0f;
            }
        }
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
                _state.CurrentAction = EnemyAction.Idle;
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


    #region * Helper Functions 
    // Moves the enemy character to target position
    private void MoveTo(Vector3 position)
    {
        _state.CurrentAction = EnemyAction.Move;
        _state.MovementTarget = position;
    }
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
    #endregion
}
