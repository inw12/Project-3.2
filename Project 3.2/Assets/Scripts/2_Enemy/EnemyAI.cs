using UnityEngine;

public struct EnemyState
{
    public EnemyAction CurrentAction;
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
    [Header("State Machine Control")]
    [SerializeField] private float stateChangeCooldown = 5f;
    private float _cooldownTimer;

    [Header("Enemy Movement")]
    [SerializeField] private float movementRange = 25f;

    private EnemyState _state;
    private EnemyState _prevState;

    private Rigidbody _rb;

    // Start()
    public void Initialize()
    {
        _state.CurrentAction = EnemyAction.Idle;
        _prevState = _state;

        _rb = GetComponent<Rigidbody>();

        _cooldownTimer = 0f;
    }

    // Update()
    public void UpdateAI(float deltaTime)
    {
        if (_state.CurrentAction is EnemyAction.Idle) _cooldownTimer += deltaTime;
        if (_cooldownTimer >= stateChangeCooldown)
        {
            // "do something..."

            _cooldownTimer = 0f;
        }
    }

    // FixedUpdate()
    public void UpdateMovement(float fixedDeltaTime)
    {
        
    }


    #region *--- Helper Functions --------------------------------------------------*
    // Returns a random Vector3 position within given radius projected onto y-axis
    private Vector3 GetRandomPosition(float radius)
    {
        var target = Random.insideUnitSphere * radius;
        target = Vector3.ProjectOnPlane(target, Vector3.up);
        return target;
    }
    #endregion
}
