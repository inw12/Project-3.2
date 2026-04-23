/// ************************************************
/// * Animator Parameters:
///     - xVelocity             (float)
///     - yVelocity             (float)
///     - MovementAction        (int)
///     - CombatAction          (int)
///     - MeleeTrigger          (Trigger)
///     - ComboCount            (int)
///     - HitstunActive         (bool)
///     - ParryTrigger          (Trigger)
/// ************************************************

using UnityEngine;
[RequireComponent(typeof(Animator))]
public class PlayerAnimationController : MonoBehaviour
{
    [Header("Components Requiring Animation Control")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerCombat playerCombat;

    private Animator _animator;

    // Action States
    private MovementState _moveState;
    private MovementState _prevMoveState;
    private CombatState _combatState;
    private CombatState _prevCombatState;

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
        _combatState = playerCombat.GetState();

        // Update Velocity
        var velocity = transform.InverseTransformDirection(_moveState.Velocity.normalized);
        _animator.SetFloat("xVelocity", velocity.x);
        _animator.SetFloat("yVelocity", velocity.z);

        // Movement Action
        if (_prevMoveState.CurrentAction != _moveState.CurrentAction) {
            _animator.SetInteger("MovementAction", (int)_moveState.CurrentAction);
        }

        // Combat Action
        if (_prevCombatState.CurrentAction != _combatState.CurrentAction) {
            _animator.SetInteger("CombatAction", (int)_combatState.CurrentAction);
        }

        _prevMoveState = _moveState;
        _prevCombatState = _combatState;
    }

    // Triggers Melee attack animation & updates "ComboCount"
    public void TriggerMeleeAnimation(int combo)
    {
        _animator.SetTrigger("MeleeTrigger");
        _animator.SetInteger("ComboCount", combo);
    }

    // Toggles "HitstunActive" bool
    public void HitstunEnabled(bool b)
    {
        _animator.SetBool("HitstunActive", b);
    }

    // Triggers "ParryTrigger"
    public void TriggerParry()
    {
        _animator.SetTrigger("ParryTrigger");
    }
}
