using UnityEngine;
public class ChargeActiveOnEnter : StateMachineBehaviour
{
    private static readonly int ChargeActiveHash = Animator.StringToHash("ChargeActive");

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool(ChargeActiveHash, true);
    }
}
