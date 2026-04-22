/// ************************************************
/// * Animator Parameters:
///     - xVelocity             (float)
///     - yVelocity             (float)
///     - MovementAction        (int)
///     - CombatAction          (int)
///     - MeleeTrigger          (Trigger)
///     - ComboCount            (int)
/// ************************************************

using UnityEngine;
[RequireComponent(typeof(Animator))]
public class PlayerAnimationController : MonoBehaviour
{
    [Header("Components Requiring Animation Control")]
    [SerializeField] private PlayerMovement playerMovement;

    private Animator _animator;

    // Action States
    private MovementState _moveState;
    private MovementState _prevMoveState;

    public void Initialize()
    {
        _animator = GetComponent<Animator>();

        // Initialize Animator Values
        _animator.SetInteger("MovementAction", (int)_moveState.CurrentAction);
    }

    /// 'Player.cs'
    ///     L LateUpdate()
    public void UpdateAnimator()
    {
        _moveState = playerMovement.GetState();

        // Update Velocity
        var velocity = transform.InverseTransformDirection(_moveState.Velocity.normalized);
        _animator.SetFloat("xVelocity", velocity.x);
        _animator.SetFloat("yVelocity", velocity.z);

        // Movement Action
        if (_prevMoveState.CurrentAction != _moveState.CurrentAction) {
            _animator.SetInteger("MovementAction", (int)_moveState.CurrentAction);
        }

        _prevMoveState = _moveState;
    }

    public void TriggerMeleeAnimation(int combo)
    {
        _animator.SetTrigger("MeleeTrigger");
        _animator.SetInteger("ComboCount", combo);
    }
}
