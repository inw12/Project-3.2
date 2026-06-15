using System;
using UnityEngine;
[RequireComponent(typeof(Animator))]
public class PlayerAnimationController : MonoBehaviour
{
    [Header("Components Requiring Animation Control")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerCombat playerCombat;

    private Animator _animator;

    // Animator Parameters
    private static readonly int xVelocity       = Animator.StringToHash("xVelocity");
    private static readonly int yVelocity       = Animator.StringToHash("yVelocity");
    private static readonly int MovementAction  = Animator.StringToHash("MovementAction");
    private static readonly int CombatAction    = Animator.StringToHash("CombatAction");
    private static readonly int MeleeTrigger    = Animator.StringToHash("MeleeTrigger");
    private static readonly int HitstunActive   = Animator.StringToHash("HitstunActive");
    private static readonly int ParryTrigger    = Animator.StringToHash("ParryTrigger");
    private static readonly int HitTrigger      = Animator.StringToHash("HitTrigger");    

    // Action States
    private MovementState _moveState;
    private MovementState _prevMoveState;
    private CombatState _combatState;
    private CombatState _prevCombatState;

    public void Initialize()
    {
        _animator = GetComponent<Animator>();

        // Initialize Animator Values
        _animator.SetInteger(MovementAction, (int)_moveState.CurrentAction);
    }

    /// 'Player.cs'
    ///     L LateUpdate()
    public void UpdateAnimator()
    {
        _moveState = playerMovement.GetState();
        _combatState = playerCombat.GetState();

        // Update Velocity
        var velocity = transform.InverseTransformDirection(_moveState.Velocity.normalized);
        var x = MathF.Round(velocity.x, 2);
        var y = MathF.Round(velocity.z, 2);
        _animator.SetFloat(xVelocity, x);
        _animator.SetFloat(yVelocity, y);

        // Movement Action
        if (_prevMoveState.CurrentAction != _moveState.CurrentAction) {
            _animator.SetInteger(MovementAction, (int)_moveState.CurrentAction);
        }

        // Combat Action
        if (_prevCombatState.CurrentAction != _combatState.CurrentAction) {
            _animator.SetInteger(CombatAction, (int)_combatState.CurrentAction);
        }

        _prevMoveState = _moveState;
        _prevCombatState = _combatState;
    }

    #region * Animator Gateways
    // Int
    public void SetInteger(string s, int i) => _animator.SetInteger(s, i);
    public void SetInteger(int i, int j) => _animator.SetInteger(i, j);
    // Bool
    public bool GetBoolean(string s) => _animator.GetBool(s);
    public void SetBoolean(string s, bool b) => _animator.SetBool(s, b);
    // Trigger
    public void SetTrigger(string s) => _animator.SetTrigger(s);
    public void SetTrigger(int i) => _animator.SetTrigger(i);
    #endregion

    // Toggles "HitstunActive" bool
    public void SetHitstunActive(bool b)
    {
        _animator.SetBool(HitstunActive, b);
    }

    // Triggers "ParryTrigger"
    public void TriggerParry()
    {
        _animator.SetTrigger(ParryTrigger);
    }

    // Sets Animator to 'Idle' and resets most parameters
    public void SetToIdle() 
    {
        _animator.Play("Idle");
        ResetParameters();
    }    

    public void ResetParameters()
    {
        _animator.SetFloat(xVelocity, 0f);
        _animator.SetFloat(yVelocity, 0f);
        _animator.SetInteger(MovementAction, 0);
        _animator.SetInteger(CombatAction, 0);
        _animator.SetBool(HitstunActive, false);
    }

    public void SetSpeed(float f) => _animator.speed = f;
}
