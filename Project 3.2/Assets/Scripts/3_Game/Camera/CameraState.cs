using UnityEngine;
public abstract class CameraState : MonoBehaviour
{
    public abstract Vector3 GetTargetPosition();
    public abstract Quaternion GetTargetRotation();
    public virtual void OnStateEnter() {}
    public virtual void OnStateExit() {}
}
