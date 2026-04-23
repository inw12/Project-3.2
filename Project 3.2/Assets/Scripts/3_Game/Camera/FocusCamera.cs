using UnityEngine;
public class FocusCamera : CameraState
{
    [SerializeField] private Vector3 cameraPosition;
    [SerializeField] private Vector3 cameraRotation;

    public override Vector3 GetTargetPosition() 
    {
        var targetPosition = cameraPosition;
        if (Player.Instance)
        {
            targetPosition += Player.Instance.transform.position;
        }
        return targetPosition;
    }
    public override Quaternion GetTargetRotation() => Quaternion.Euler(cameraRotation);
}
