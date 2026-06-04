using UnityEngine;
public class AttackActiveToggle : StateMachineBehaviour
{
    private static readonly int AttackActiveHash = Animator.StringToHash("AttackActive");

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool(AttackActiveHash, true);
    }
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool(AttackActiveHash, false);
    }
}
