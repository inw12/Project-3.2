using UnityEngine;

public struct MovementState
{
    public MovementAction CurrentAction;
    public Vector3 Velocity;
    public bool IsGrounded;
}
public enum MovementAction
{
    Idle    = 0,
    Move    = 1,
    Roll    = 2
}
public struct MovementInput
{
    public Vector2 Movement;
    public Vector3 MousePosition;
    public bool Roll;
}

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float moveAcceleration = 20f;
    [SerializeField] private float moveRotation = 15f;

    [Header("Roll")]
    [SerializeField] private CapsuleCollider hurtbox;
    [SerializeField] private float rollSpeed = 12;
    [SerializeField] private float rollAcceleration = 20f;
    [SerializeField] private float rollDuration = 0.5f;

    private CharacterController _controller;
    private bool _movementInputEnabled;

    // Requested Inputs
    private Vector3 _requestedMovement;
    private Vector3 _requestedMousePosition;
    private bool _requestedRoll;

    // State Machine
    private MovementState _state;
    private MovementState _prevState;

    // Roll Variables
    private struct RollData
    {
        public Vector3 Direction;
        public bool Triggered;
        public float Timer;
    }
    private RollData _rollData;


    // Start()
    public void Initialize()
    {
        _controller = GetComponent<CharacterController>();

        _movementInputEnabled = true;

        _state.CurrentAction = MovementAction.Idle;
        _state.Velocity = Vector3.zero;
        _state.IsGrounded = _controller.isGrounded;
        _prevState = _state;
    }

    // Update()
    public void UpdateInput(MovementInput input)
    {
        if (_movementInputEnabled)
        {
            _requestedMovement = new Vector3(input.Movement.x, 0f, input.Movement.y).normalized;

            _requestedRoll = input.Roll;
            TryTriggerRoll();

            _requestedMousePosition = input.MousePosition;
        }
    }

    // FixedUpdate()
    public void UpdateMovement(float fixedDeltaTime)
    {
        ApplyGravity();

        // Roll Movement 
        // * roll triggered *
        if (_rollData.Triggered)
        {
            _state.CurrentAction = MovementAction.Roll;
            _rollData.Timer += fixedDeltaTime;

            // Sustain dodge velocity during duration
            var targetVelocity = rollSpeed * _rollData.Direction;
            _state.Velocity = Vector3.Lerp
            (
                _state.Velocity,
                targetVelocity,
                1f - Mathf.Exp(-rollAcceleration * fixedDeltaTime)
            );

            // Reset everything once dodge duration reached
            if (_rollData.Timer > rollDuration)
            {
                _rollData.Triggered = false;
                _movementInputEnabled = true;
                hurtbox.enabled = true;
            }
        }
        // Regular Movement
        // * movement input *
        else if (_requestedMovement.sqrMagnitude > 0f && _movementInputEnabled)
        {
            _state.CurrentAction = MovementAction.Move;

            var targetVelocity = moveSpeed * _requestedMovement;
            _state.Velocity = Vector3.Lerp
            (
                _state.Velocity,
                targetVelocity,
                1f - Mathf.Exp(-moveAcceleration * fixedDeltaTime)
            );
        }
        // Idle
        // * no input *
        else if (_requestedMovement.sqrMagnitude == 0f && _movementInputEnabled)
        {
            _state.CurrentAction = MovementAction.Idle;
            _state.Velocity = Vector3.Lerp
            (
                _state.Velocity,
                Vector3.zero,
                1f - Mathf.Exp(-moveAcceleration * fixedDeltaTime)
            );
        }
        

        // Apply Movement
        if (_controller.enabled) _controller.Move(_state.Velocity * fixedDeltaTime);

        // Update State Machine
        _prevState = _state;
    }

    // LateUpdate()
    public void UpdateRotation(float deltaTime)
    {
        // (A) Rotate towards mouse position during Ranged Attack
        if (Player.Instance.GetCurrentCombatAction() is CombatAction.Ranged)
        {
            var targetRotation = Quaternion.LookRotation(_requestedMousePosition);
            transform.rotation = Quaternion.Lerp
            (
                transform.rotation,
                targetRotation,
                1f - Mathf.Exp(-moveRotation * deltaTime)
            );
        }
        // (B) Do nothing. Rotation directly set from 'MeleeAttack.cs'
        else if (Player.Instance.GetCurrentCombatAction() is CombatAction.Melee)
        {}
        // (C) Rotate towards direction of movement while not Attacking
        else if (_requestedMovement.sqrMagnitude > 0f)
        {
            var targetRotation = Quaternion.LookRotation(_requestedMovement);
            transform.rotation = Quaternion.Lerp
            (
                transform.rotation,
                targetRotation,
                1f - Mathf.Exp(-moveRotation * deltaTime)
            );
        }
    }


    #region *--- Helper Functions --------------------------------------------------*
    private void ApplyGravity()
    {
        // Gravity
        _state.IsGrounded = _controller.isGrounded;
        if (_state.IsGrounded)
        {
            if (_prevState.Velocity.y < -2f) {
                _state.Velocity.y = -2f;
            }
        } else {
            _state.Velocity += 2 * Time.deltaTime * Physics.gravity;
        }
    }
    private void TryTriggerRoll()
    {
        if (_requestedRoll && !_rollData.Triggered && _requestedMovement.sqrMagnitude > 0f)
        {
            // Update dodge info
            _rollData.Triggered = true;
            _rollData.Direction = _requestedMovement;
            _rollData.Timer = 0f;

            // Disable hurtbox
            hurtbox.enabled = false;

            // Disable player inputs
            _movementInputEnabled = false;
        }
    }
    #endregion


    #region *--- Public Getters ----------------------------------------------------*
    public MovementState GetState() => _state;
    public MovementState GetPrevState() => _prevState;
    #endregion


    #region *--- Public Methods used by other classes to influence Player movement -----*
    public void MovementInputEnabled(bool b)
    {
        if (!b)
        {
            // Disable Input
            _movementInputEnabled = b;

            // Stop any character movement
            _requestedMovement = _state.Velocity = Vector3.zero;
            _state.CurrentAction = MovementAction.Idle;
        }
        else
        {
            _movementInputEnabled = b;
        }
    }
    public void SetVelocity(Vector3 velocity, float acceleration)
    {
        _state.Velocity = Vector3.Lerp
        (
            _state.Velocity,
            velocity,
            1f - Mathf.Exp(-acceleration * Time.fixedDeltaTime)
        );
    }
    public void SetRotation(Quaternion rotation) => transform.rotation = rotation;
    #endregion
}
