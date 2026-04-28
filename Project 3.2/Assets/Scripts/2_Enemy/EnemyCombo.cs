using System.Collections.Generic;
using UnityEngine;
public class EnemyCombo : MonoBehaviour
{
    public bool ShowDebug;

    [SerializeField] private float damage;
    [Space]
    [SerializeField] private Transform hitboxSpawn;
    [SerializeField] private float hitboxRadius;
    [Space]
    [SerializeField] private LayerMask targetLayer;

    private bool _hitboxEnabled;
    private readonly Collider[] _hits   = new Collider[10];   
    private readonly HashSet<Collider> _alreadyHit = new(); 

    void OnDrawGizmos()
    {
        if (!ShowDebug) return;

        if (_hitboxEnabled)
        {
            Gizmos.color = Color.lightCyan;
            Gizmos.DrawWireSphere(hitboxSpawn.position, hitboxRadius);
        }
    }

    void Update()
    {
        if (_hitboxEnabled)
        {
            // Scan for collisions
            var hits = Physics.OverlapSphereNonAlloc
            (
                hitboxSpawn.position,
                hitboxRadius,
                _hits,
                targetLayer
            );

            // Trigger hit 
            if (hits > 0)
            {
                var hit = _hits[0];

                if (_alreadyHit.Add(hit))
                {
                    if (hit.TryGetComponent(out IDamageable e))
                    {
                        e.DecreaseHealth(damage);
                    }
                }
            }
        }
    }

    // for the Animator...
    public void EnableHitbox()
    {
        _alreadyHit.Clear();
        _hitboxEnabled = true;
    }
    public void DisableHitbox()
    {
        _hitboxEnabled = false;
    }
}
