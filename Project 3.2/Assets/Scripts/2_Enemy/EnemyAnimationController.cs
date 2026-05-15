using UnityEngine;
[RequireComponent(typeof(Animator))]
public class EnemyAnimationController : MonoBehaviour
{
    private Animator _animator;

    // Animator Parameters
    private static readonly int CurrentAction       = Animator.StringToHash("CurrentAction");
    private static readonly int AttackID            = Animator.StringToHash("AttackID");
    private static readonly int AttackTrigger       = Animator.StringToHash("AttackTrigger");    
    private static readonly int AttackActive        = Animator.StringToHash("AttackActive");    
    private static readonly int ComboTrigger        = Animator.StringToHash("ComboTrigger");
    private static readonly int KnockbackTrigger    = Animator.StringToHash("KnockbackTrigger");
    private static readonly int InHitstun           = Animator.StringToHash("InHitstun");
    private static readonly int PullTrigger         = Animator.StringToHash("PullTrigger");
    private static readonly int PlayerPulled        = Animator.StringToHash("PlayerPulled");
    private static readonly int InParryPhase        = Animator.StringToHash("InParryPhase");

    public void Initialize()
    {
        _animator = GetComponent<Animator>();
    }

    public void UpdateAnimator()
    {
        
    }
}
