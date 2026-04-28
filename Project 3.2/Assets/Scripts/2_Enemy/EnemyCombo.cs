using System;
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
    private int _hurtboxLayer;  // player hurtbox
    private int _parryboxLayer; // player parrybox

    private bool _hitboxEnabled;
    private readonly Collider[] _hits   = new Collider[10];   
    private readonly HashSet<Collider> _alreadyHit = new(); 

    private int _comboCount;
    private int _parryCount;

    public event Action OnComboEnd;

    void OnDrawGizmos()
    {
        if (!ShowDebug) return;

        if (_hitboxEnabled)
        {
            Gizmos.color = Color.lightCyan;
            Gizmos.DrawWireSphere(hitboxSpawn.position, hitboxRadius);
        }
    }

    void OnGUI()
    {
        if (!ShowDebug) return;

        GUILayout.Label($"Hits: {_comboCount}");
        GUILayout.Label($"Parries: {_parryCount}");
    }

    void Awake()
    {
        _hurtboxLayer = LayerMask.NameToLayer("PlayerHurtbox");
        _parryboxLayer = LayerMask.NameToLayer("PlayerParrybox");

        _comboCount = _parryCount = 0;
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

            // Trigger hit (if detected)
            if (hits > 0)
            {
                var hit = _hits[0];
                var layer = hit.gameObject.layer;

                if (_alreadyHit.Add(hit))
                {
                    // PARRY Collisions
                    if (layer == _parryboxLayer)
                    {
                        if (hit.TryGetComponent(out IParrybox p))
                        {
                            p.TriggerParry();

                            _parryCount++;
                        }
                    }
                    // HURTBOX Collisions
                    else if (layer == _hurtboxLayer)
                    {
                        if (hit.TryGetComponent(out IDamageable e))
                        {
                            e.DecreaseHealth(damage);
                        }
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
        _comboCount++;
    }
    public void DisableHitbox()
    {
        _hitboxEnabled = false;
    }
    public void CheckForExit()  // check for end of combo phase
    {
        OnComboEnd?.Invoke();
    }
}
