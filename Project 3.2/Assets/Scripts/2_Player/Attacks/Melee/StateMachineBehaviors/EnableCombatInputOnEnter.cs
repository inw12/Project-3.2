using UnityEngine;
public class EnableCombatInputOnEnter : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Player.Instance.CombatInputEnabled(true);
    }
}
