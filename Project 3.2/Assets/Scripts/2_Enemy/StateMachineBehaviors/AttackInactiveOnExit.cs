using UnityEngine;
public class AttackInactiveOnExit : StateMachineBehaviour
{
    private static readonly int AttackActiveHash = Animator.StringToHash("AttackActive");

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool(AttackActiveHash, false);
    }
}