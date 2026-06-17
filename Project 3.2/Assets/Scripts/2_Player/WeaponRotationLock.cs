using UnityEngine;
public class WeaponRotationLock : MonoBehaviour
{
    [SerializeField] private PlayerCombat playerCombat;
    private CombatState _state;
    private bool _rotationLockOn;
    private CombatAction _prevAction;
    private Vector3 _prevTarget;

    void Update()
    {
        if (playerCombat == null)
        {
            enabled = false;
            return;
        }

        _state = playerCombat.GetState();
        var action = _state.CurrentAction;
        var target = _state.Target;

        // Avoid work if nothing relevant changed and rotation isn't active
        if (action == _prevAction && target == _prevTarget && !_rotationLockOn) return;

        _prevAction = action;
        _prevTarget = target;

        // While shooting, lock rotation towards cursor
        if (action is CombatAction.Ranged)
        {
            _rotationLockOn = true;

            var start = transform.position;
            start.y = 0f;

            var end = target;
            end.y = 0f;

            var forward = (end - start).normalized;
            transform.rotation = Quaternion.LookRotation(forward);
        }
        // Otherwise, reset local rotation to be Vector3.zero 
        else
        {
            if (_rotationLockOn)
            {
                _rotationLockOn = false;
                transform.localRotation = Quaternion.Euler(Vector3.zero);
            }
        }
    }
}
