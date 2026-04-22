using UnityEngine;
public class ToggleMeleeHitbox : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Player.Instance.MeleeHitboxEnabled(true);
    }
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Player.Instance.MeleeHitboxEnabled(false);
    }
}
