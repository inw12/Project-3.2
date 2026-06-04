using UnityEngine;
using UnityEngine.Animations.Rigging;

public class EnemyArmRig : MonoBehaviour
{
    public bool ShowDebug;
    [Header("Components Influencing this Animation Rig")]
    [SerializeField] private EnemyAI enemyAI;

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
    [Space]
    [SerializeField] private Transform handBone;
    [SerializeField] private Vector3 handRotationOffset;

    private bool _rigActive;

    void OnDrawGizmos()
    {
        if (!ShowDebug) return;

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(target.position, 0.5f);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(hint.position, 0.25f);
    }
    void OnGUI()
    {
        if (!ShowDebug) return;

        var debugText = $"Rig Active: {_rigActive}\n"
                        + $"Shoulder Weight: {shoulderAim.weight}\n"
                        + $"Arm Weight: {armAim.weight}\n";
        GUI.Label(new Rect(10, 200, 300, 100), debugText);
    }

    public void Initialize()
    {
        shoulderAim.weight = 0f;
        armAim.weight = 0f;

        target.position = Vector3.zero;
        hint.position = Vector3.zero;

        _rigActive = false;
    }

    public void UpdateRig()
    {
        if (_rigActive)
            RaiseArm();
        else 
            LowerArm();
    }

    public void ArmRigEnabled(bool b) => _rigActive = b;

    private void RaiseArm()
    {
        // Update TARGET position
        target.position = enemyAI.GetState().PlayerPosition;

        // Update HINT position
        var shoulder = shoulderAim.data.constrainedObject.transform;
        var forward = (target.position - shoulder.position).normalized;
        var right = Vector3.Cross(Vector3.up, forward);
        var up = Vector3.Cross(forward, right);
        hint.position = shoulder.position
                        + forward   * elbowOffset.z
                        + up        * elbowOffset.y
                        + right     * elbowOffset.x;

        // Update WEIGHT
        shoulderAim.weight = armAim.weight = Mathf.Lerp
        (
            shoulderAim.weight,
            targetWeight,
            1f - Mathf.Exp(-animationSpeed * Time.deltaTime)
        );

        // Hand ROTATION
        if (handBone)
        {
            var direction = (target.position - handBone.position).normalized;
            handBone.rotation = Quaternion.LookRotation(direction, Vector3.up)
                                * Quaternion.Euler(handRotationOffset);
        }
    }

    private void LowerArm()
    {
        // * Lerp WEIGHT values to 0
        if (shoulderAim.weight > 0f || armAim.weight > 0f)
        {
            shoulderAim.weight = armAim.weight = Mathf.Lerp
            (
                shoulderAim.weight,
                0f,
                1f - Mathf.Exp(-animationSpeed * Time.deltaTime)
            );
        }
    }
}
