using UnityEngine;
public class CameraManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [Space]
    [SerializeField] private float positionLerpSpeed = 5f;
    [SerializeField] private float rotationLerpSpeed = 5f;

    private CameraState _currentState;

    void Start()
    {
        SwitchTo<DefaultCamera>();
    }

    void LateUpdate()
    {
        var deltaTime = Time.deltaTime;

        if (!_currentState) return;
        
        mainCamera.transform.SetPositionAndRotation
        (
            Vector3.Lerp
            (
                mainCamera.transform.position,
                _currentState.GetTargetPosition(),
                1f - Mathf.Exp(-positionLerpSpeed * deltaTime)
            ),
            Quaternion.Slerp
            (
                mainCamera.transform.rotation,
                _currentState.GetTargetRotation(),
                1f - Mathf.Exp(-rotationLerpSpeed* deltaTime)
            )
        );
    }

    public void SwitchTo<T>() where T : CameraState
    {
        // Try to switch current state
        CameraState next = GetComponentInChildren<T>();
        if (!next || next == _currentState) return;
        
        // State Transition
        _currentState?.OnStateExit();
        _currentState = next;
        _currentState?.OnStateEnter();
    }
}
