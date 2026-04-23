///
/// *** This script controls the animation rig
///     that controls the character's LEFT ARM
///     to shoot projectiles
/// 
using UnityEngine;
using UnityEngine.Animations.Rigging;
public class PlayerAnimationRig : MonoBehaviour
{
    [Header("Components Influencing this Animation Rig")]
    [SerializeField] private PlayerCombat playerCombat;

    [Header("Animation Rig Components")]
    [SerializeField] private MultiAimConstraint shoulderAim;
    [SerializeField] private TwoBoneIKConstraint armAim;
    [Space]
    [SerializeField] private Transform target;
    [SerializeField] private Transform hint;

    [Header("Animation Rig Settings")]
    [SerializeField] [Range(0f, 1f)] private float targetWeight = 1f;
    [SerializeField] private float animationSpeed = 10f;
    [SerializeField] private Vector3 elbowOffset;

    private CombatState _state;
    private bool _rigActive;

    public void Initialize()
    {
        shoulderAim.weight = 0f;
        armAim.weight = 0f;

        target.position = Vector3.zero;
        hint.position = Vector3.zero;

        _rigActive = false;
    }

    // * This rig will toggle between ON and OFF depending on
    //   if the player is currently performing a RANGED ATTACK
    public void UpdateRig()
    {
        _state = playerCombat.GetState();

        _rigActive = _state.CurrentAction is CombatAction.Ranged;

        if (_rigActive)
            RaiseArm();
        else 
            LowerArm();
    }

    private void RaiseArm()
    {
        // Update TARGET position
        target.position = _state.Target;

        // Update HINT position
        var shoulder = shoulderAim.data.constrainedObject.transform;
        var direction = (target.position - shoulder.position).normalized;
        var targetPosition = shoulder.position
                            + direction * elbowOffset.z     // forward offset
                            + Vector3.up * elbowOffset.y    // up/down offset
                            + Vector3.left * elbowOffset.x; // left/right offset
        hint.position = targetPosition;

        // Lerp WEIGHT values to 100
        shoulderAim.weight = armAim.weight = Mathf.Lerp
        (
            shoulderAim.weight,
            targetWeight,
            1f - Mathf.Exp(-animationSpeed * Time.deltaTime)
        );
    }

    private void LowerArm()
    {
        // * Lerp WEIGHT values to 0
        shoulderAim.weight = armAim.weight = Mathf.Lerp
        (
            shoulderAim.weight,
            0f,
            1f - Mathf.Exp(-animationSpeed * Time.deltaTime)
        );
    }
}
