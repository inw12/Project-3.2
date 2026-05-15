using UnityEngine;

public struct EnemyAnimatorContext
{
    public int CurrentAction;
    public int AttackID;
    public bool AttackActive;
    public bool InHitstun;
}

[RequireComponent(typeof(Animator))]
public class EnemyAnimationController : MonoBehaviour
{
    private Animator _animator;

    #region *--- Animator Parameters ------------------------------------------------------------*
    //
    // * "High Level" Parameters
    //      - Parameters untouched by GameManagers
    private static readonly int CurrentAction       = Animator.StringToHash("CurrentAction");
    private static readonly int AttackID            = Animator.StringToHash("AttackID");
    private static readonly int AttackTrigger       = Animator.StringToHash("AttackTrigger");    
    private static readonly int AttackActive        = Animator.StringToHash("AttackActive");    
    private static readonly int KnockbackTrigger    = Animator.StringToHash("KnockbackTrigger");
    private static readonly int InHitstun           = Animator.StringToHash("InHitstun");

    // * "Low Level" Parameters
    //      - Parameters used by GameManagers
    private static readonly int PullTrigger         = Animator.StringToHash("PullTrigger");
    private static readonly int PlayerPulled        = Animator.StringToHash("PlayerPulled");
    private static readonly int ComboTrigger        = Animator.StringToHash("ComboTrigger");
    private static readonly int InParryPhase        = Animator.StringToHash("InParryPhase");
    //
    #endregion

    public void Initialize()
    {
        _animator = GetComponent<Animator>();
    }

    public void UpdateAnimator(EnemyAnimatorContext context)
    {
        _animator.SetInteger(CurrentAction, context.CurrentAction);
        _animator.SetInteger(AttackID, context.AttackID);
        _animator.SetBool(AttackActive, context.AttackActive);
        _animator.SetBool(InHitstun, context.InHitstun);
    }

    #region *--- Public Setters --------------------------------------------------*
    public void SetInteger(string s, int i) => _animator.SetInteger(s, i);
    public void SetFloat(string s, int i)   => _animator.SetFloat(s, i);
    public void SetBool(string s, bool b)   => _animator.SetBool(s, b);
    public void SetTrigger(string s)        => _animator.SetTrigger(s);
    #endregion
}
