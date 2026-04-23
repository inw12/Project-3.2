using UnityEngine;
public interface IKnockable
{
    void TriggerKnockback(Vector3 direction, float force, float duration);
}
