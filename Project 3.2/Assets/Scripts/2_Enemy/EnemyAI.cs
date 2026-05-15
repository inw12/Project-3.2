using UnityEngine;

public struct EnemyState
{
    public EnemyAction CurrentAction;

    public int AttackID;
    public bool AttackActive;

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

    private Rigidbody _rb;

    private bool _isActive;

    // Debug Gizmos
    void OnDrawGizmosSelected()
    {
        if (!ShowDebug) return;
        Gizmos.color = Color.red;
        if (_state.MovementTarget != Vector3.zero) Gizmos.DrawSphere(_state.MovementTarget, 0.5f);
    }

    // Start()
    public void Initialize()
    {
        _rb = GetComponent<Rigidbody>();

        SetToIdle();
        _isActive = true;
    }

    // Update()
    public void UpdateAI(float deltaTime)
    {
        if (_isActive)
        {
            if (_state.CurrentAction is EnemyAction.Idle) _cooldownTimer += deltaTime;
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
        // Movement Implementation
        if (_state.CurrentAction is EnemyAction.Move)
        {
            // If destination reached, exit movement state
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


    #region *--- Helper Functions --------------------------------------------------*
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


    #region *--- Public Accessors --------------------------------------------------*
    // State Getters
    public EnemyState GetState() => _state;
    public EnemyState GetPrevState() => _prevState;
    // Enable/Disable Enemy AI
    public void EnemyActive(bool b) => _isActive = b;
    // Reset Enemy AI
    public void SetToIdle()
    {
        _state.CurrentAction = EnemyAction.Idle;
        _state.AttackID = 0;
        _state.AttackActive = false;
        _state.MovementTarget = Vector3.zero;

        _cooldownTimer = 0f;
    }
    #endregion
}
