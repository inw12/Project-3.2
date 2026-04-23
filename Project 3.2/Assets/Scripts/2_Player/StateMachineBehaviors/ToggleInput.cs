using UnityEngine;
public class ToggleInput : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Player.Instance.MovementInputEnabled(false);
        Player.Instance.CombatInputEnabled(false);
    }
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Player.Instance.MovementInputEnabled(true);
        Player.Instance.CombatInputEnabled(true);
    }
}
