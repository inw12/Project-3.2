///
/// *** This script controls the animation rig
///     that controls the character's LEFT ARM
///     to shoot projectiles
/// 
using UnityEngine;
using UnityEngine.Animations.Rigging;
public class PlayerAnimationRig : MonoBehaviour
{
    public bool ShowDebug;

    [Header("Components Influencing this Animation Rig")]
    [SerializeField] private PlayerCombat playerCombat;

    [Header("Constraints | Left Arm")]
    public bool leftArmActive;
    [SerializeField] private MultiAimConstraint shoulderL;
    [SerializeField] private TwoBoneIKConstraint armL;
    [SerializeField] private Transform targetL;
    [SerializeField] private Transform hintL;
    [SerializeField] private Vector3 hintOffsetL;
    [Space]
    [SerializeField] private Transform handBoneL;
    [SerializeField] private Vector3 handRotationOffsetL;

    [Header("Constraints | Right Arm")]
    public bool rightArmActive;
    [SerializeField] private MultiAimConstraint shoulderR;
    [SerializeField] private TwoBoneIKConstraint armR;
    [SerializeField] private Transform targetR;
    [SerializeField] private Transform hintR;
    [SerializeField] private Vector3 hintOffsetR;
    [Space]
    [SerializeField] private Transform handBoneR;
    [SerializeField] private Vector3 handRotationOffsetR;
    [Space]
    [SerializeField] private float targetRadiusR;
    [SerializeField] private float targetHeightR;

    [Header("Animation Rig Settings")]
    [SerializeField] private float animationSpeed = 10f;
    [SerializeField] [Range(0f, 1f)] private float targetWeight = 1f;

    private CombatState _state;
    private bool _rigActive;

    #region * Debugging
    void OnDrawGizmos()
    {
        if (!ShowDebug) return;
        // Target
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(targetR.position, 0.1f);
        // Hint
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(hintR.position, 0.05f);
    }
    #endregion


    public void Initialize()
    {
        shoulderL.weight = 0f;
        armL.weight = 0f;
        shoulderR.weight = 0f;
        armR.weight = 0f;

        targetL.position = Vector3.zero;
        targetR.position = Vector3.zero;
        hintL.position = Vector3.zero;
        hintR.position = Vector3.zero;

        _rigActive = false;
    }

    // * This rig will toggle between ON and OFF depending on
    //   if the player is currently performing a RANGED ATTACK
    public void UpdateRig()
    {
        _state = playerCombat.GetState();
        _rigActive = _state.CurrentAction is CombatAction.Ranged;

        hintR.localPosition = hintOffsetR;

        if (_rigActive)
        {
            RaiseLeft();
            RaiseRight();
        }
        else
        {
            LowerArms();
        } 
    }

    private void RaiseLeft()
    {
        if (leftArmActive)
        {
            // TARGET
            targetL.position = _state.Target;

            // HINT
            var shoulder = shoulderL.data.constrainedObject.transform;
            var direction = (targetL.position - shoulder.position).normalized;
            var targetPosition = shoulder.position
                                + direction * hintOffsetL.z     // forward offset
                                + Vector3.up * hintOffsetL.y    // up/down offset
                                + Vector3.left * hintOffsetL.x; // left/right offset
            hintL.position = targetPosition;

            // APPLY Changes
            shoulderL.weight = armL.weight = Mathf.Lerp
            (
                shoulderL.weight,
                targetWeight,
                1f - Mathf.Exp(-animationSpeed * Time.deltaTime)
            );

            // Force hand to desired rotation
            if (handBoneL)
            {
                var handDirection = (targetL.position - handBoneL.position).normalized;
                handBoneL.rotation = Quaternion.LookRotation(handDirection, Vector3.up)
                                    * Quaternion.Euler(handRotationOffsetL);
            }
        }
    }

    private void RaiseRight()
    {
        if (rightArmActive)
        {
            // TARGET
            var start = transform.position;
            var end = _state.Target;
            start.y = end.y = targetHeightR;
            
            var distance = end - start;
            targetR.position = start + Vector3.ClampMagnitude(distance, targetRadiusR);

            // HINT
            //hintR.localPosition = hintOffsetR;

            // APPLY Changes
            shoulderR.weight = armR.weight = Mathf.Lerp
            (
                shoulderR.weight,
                targetWeight,
                1f - Mathf.Exp(-animationSpeed * Time.deltaTime)
            );

            // Force hand to desired rotation
            if (handBoneR)
            {
                var handDirection = (targetR.position - handBoneR.position).normalized;
                handBoneR.rotation = Quaternion.LookRotation(handDirection, Vector3.up)
                                    * Quaternion.Euler(handRotationOffsetR);
            }
        }
    }

    private void LowerArms()
    {
        shoulderL.weight = armL.weight = Mathf.Lerp
        (
            shoulderL.weight,
            0f,
            1f - Mathf.Exp(-animationSpeed * Time.deltaTime)
        );

        shoulderR.weight = armR.weight = Mathf.Lerp
        (
            shoulderR.weight,
            0f,
            1f - Mathf.Exp(-animationSpeed * Time.deltaTime)
        );
    }
}