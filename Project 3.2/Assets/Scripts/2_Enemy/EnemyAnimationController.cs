using System;
using UnityEngine;

public struct EnemyAnimatorContext
{
    public Vector3 Velocity;
    public int CurrentAction;
    public int AttackID;
    public bool InHitstun;
}

[RequireComponent(typeof(Animator))]
public class EnemyAnimationController : MonoBehaviour
{
    #region * Animator Parameters 
    //
    // * "High Level" Parameters
    //      - Parameters used by Enemy scripts
    private static readonly int xVelocity           = Animator.StringToHash("xVelocity");
    private static readonly int yVelocity           = Animator.StringToHash("yVelocity");
    private static readonly int CurrentAction       = Animator.StringToHash("CurrentAction");
    private static readonly int AttackID            = Animator.StringToHash("AttackID");  
    private static readonly int AttackActive        = Animator.StringToHash("AttackActive");    // used to toggle attacks during animation states
    private static readonly int KnockbackTrigger    = Animator.StringToHash("KnockbackTrigger");
    private static readonly int InHitstun           = Animator.StringToHash("InHitstun");
    //
    // * "Low Level" Parameters
    //      - Parameters used by GameManager scripts
    private static readonly int PullTrigger         = Animator.StringToHash("PullTrigger");
    private static readonly int PlayerPulled        = Animator.StringToHash("PlayerPulled");
    private static readonly int ComboTrigger        = Animator.StringToHash("ComboTrigger");
    private static readonly int InParryPhase        = Animator.StringToHash("InParryPhase");
    #endregion

    private Animator _animator;

    public void Initialize()
    {
        _animator = GetComponent<Animator>();
    }

    public void UpdateAnimator(EnemyAnimatorContext context)
    {
        // Velocity
        var velocity = transform.InverseTransformDirection(context.Velocity.normalized);
        var x = MathF.Round(velocity.x, 2);
        var y = MathF.Round(velocity.z, 2);
        _animator.SetFloat(xVelocity, x);
        _animator.SetFloat(yVelocity, y);

        // Current Action
        _animator.SetInteger(CurrentAction, context.CurrentAction);
        // Attack ID
        _animator.SetInteger(AttackID, context.AttackID);
        // Hitstun
        _animator.SetBool(InHitstun, context.InHitstun);
    }

    #region * Public Access
    // Parameter Access
    public void     SetInteger(string s, int i) => _animator.SetInteger(s, i);
    public int      GetInteger(string s)        => _animator.GetInteger(s);

    public void     SetFloat(string s, float f) => _animator.SetFloat(s, f);
    public void     SetFloat(int i, float f)    => _animator.SetFloat(i, f);
    public float    GetFloat(string s)          => _animator.GetFloat(s);

    public void     SetBool(string s, bool b)   => _animator.SetBool(s, b);
    public bool     GetBool(string s)           => _animator.GetBool(s);

    public void     SetTrigger(string s)        => _animator.SetTrigger(s);

    public void     SetAttackActive(bool b)        => _animator.SetBool(AttackActive, b);

    // Play Control
    public void SetToIdle()
    {
        _animator.SetInteger(CurrentAction, 0);
        _animator.SetInteger(AttackID, 0);
        _animator.SetBool(AttackActive, false);
        _animator.SetBool(InHitstun, false);
        _animator.Play("Idle");
    }
    #endregion


    #region * Animation Events
    public void AttackActiveOn() => _animator.SetBool(AttackActive, true);
    #endregion
}
