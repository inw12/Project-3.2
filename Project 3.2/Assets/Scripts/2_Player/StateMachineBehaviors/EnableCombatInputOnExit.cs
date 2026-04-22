using UnityEngine;
public class EnableCombatInputOnExit : StateMachineBehaviour
{
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Player.Instance.CombatInputEnabled(true);
    }
}
