using UnityEngine;
public class ParryBehavior : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Player.Instance.MovementInputEnabled(false);
        Player.Instance.CombatInputEnabled(false);

        Player.Instance.SetCurrentCombatAction(CombatAction.Parry);
    }
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!animator.GetBool("InParryPhase"))
        {
            Player.Instance.MovementInputEnabled(true);
            Player.Instance.CombatInputEnabled(true);
        }
        Player.Instance.SetCurrentCombatAction(CombatAction.None);
    }
}
