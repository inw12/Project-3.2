using UnityEngine;
public interface IKnockable
{
    void TriggerKnockback();
    void TriggerKnockback(Vector3 direction, float force, float duration);
}
