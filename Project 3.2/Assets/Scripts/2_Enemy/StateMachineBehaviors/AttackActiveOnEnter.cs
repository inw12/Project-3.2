using UnityEngine;
public class AttackActiveOnEnter : StateMachineBehaviour
{
    private static readonly int AttackActiveHash = Animator.StringToHash("AttackActive");

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool(AttackActiveHash, true);
    }
}