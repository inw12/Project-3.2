using UnityEngine;
public class HitBehavior : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Player.Instance.InputEnabled(false);
        Player.Instance.ParryInputEnabled(true);
        
        Player.Instance.ExitMovementState();
        Player.Instance.ExitCombatState();
    }
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!animator.GetBool("InParryPhase")) {
            Player.Instance.InputEnabled(true);
        }
    }
}
