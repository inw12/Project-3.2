using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class PlayerCombatMelee : MonoBehaviour
{
    public bool ShowDebug;

    #region * Variables --------------------------------------------------
    [Header("Frame Data Settings")]
    [SerializeField] private PlayerAnimationController animationController;
    private static readonly int MeleeTrigger = Animator.StringToHash("MeleeTrigger");
    [Space]
    [SerializeField] private int startupFrames = 14;
    [SerializeField] private int activeFrames = 3;
    [SerializeField] private int endlagFrames = 20;
    private float StartDuraction  => (float)startupFrames   / _frameRate;
    private float ActiveDuraction => (float)activeFrames    / _frameRate;
    private float EndlagDuraction => (float)endlagFrames    / _frameRate;
    private readonly int _frameRate = 60;

    [Header("Stats")]
    [SerializeField] private float damage = 5f;
    [Space]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float acceleration = 20f;
    [SerializeField] private float duration = 0.1f;

    [Header("Hitbox")]
    [SerializeField] private Transform hitboxSpawn;
    [SerializeField] private float hitboxRadius;
    private bool _hitboxEnabled;
    private readonly HashSet<Collider> _alreadyHit = new(); // records hitbox collisions as they happen (avoids duplicate collision effects)
    
    [Header("Knockback")]
    [SerializeField] private float knockbackDuration = 0.2f;
    [SerializeField] private float knockbackStrength = 10f;

    [Header("Hitstun")]
    [SerializeField] private float hitstunDuration = 0.2f;
    private bool _hitstunActive;
    private float _hitstunTimer;

    private LayerMask _targetLayer;

    // 'OverlapSphereNonAlloc' buffers
    private readonly Collider[] _hits   = new Collider[10];   

    private bool _attackStarted;
    private bool _attackComplete;
    #endregion

    #region * Debugging --------------------------------------------------
    void OnDrawGizmos()
    {
        if (!ShowDebug) return;

        if (_hitboxEnabled)
        {
            Gizmos.color = Color.teal;
            Gizmos.DrawWireSphere(hitboxSpawn.position, hitboxRadius);
        }
    }
    #endregion

    public void Initialize(LayerMask targetLayer)
    {
        _targetLayer = targetLayer;

        ResetAttack();
    }

    #region * Attack Implementation ---------------------------------------
    // Called by 'PlayerCombat.cs' in 'OnMeleeAttack()' (called every frame while in "Melee" state)
    public void Attack(ref CombatState state)
    {
        // START
        if (!_attackStarted) AttackStart(ref state);

        // END
        if (_attackComplete) AttackEnd(ref state);
    }

    // Attack START
    private void AttackStart(ref CombatState state)
    {
        Debug.Log("a");

        _attackStarted = true;

        animationController.SetTrigger(MeleeTrigger);

        StartCoroutine(MeleeAttack());
    }

    // Attack ACTIVE
    private IEnumerator MeleeAttack()
    {
        Debug.Log("b");

        // * Start -------------------------------

        // * Active ------------------------------

        // * End ---------------------------------

        yield return new WaitForSeconds(1f);
        _attackComplete = true;
    }

    // Attack END
    private void AttackEnd(ref CombatState state)
    {
        Debug.Log("c");

        state.CurrentAction = CombatAction.None;
        ResetAttack();
    }
    #endregion
    
    #region * Helper Functions --------------------------------------------------
    private void UpdateHitbox(float deltaTime)
    {
        if (_hitboxEnabled)
        {
            // Scan for collisions
            var hits = Physics.OverlapSphereNonAlloc
            (
                hitboxSpawn.position,
                hitboxRadius,
                _hits,
                _targetLayer
            );

            // Trigger hit 
            if (hits > 0)
            {
                var hit = _hits[0];

                if (_alreadyHit.Add(hit))
                {
                    // 1. Try applying damage
                    if (hit.TryGetComponent(out IDamageable e)) {
                        e.DecreaseHealth(damage);
                    }

                    // 2. Try applying hitstun
                    if (hit.TryGetComponent(out IHitstunnable h))
                    {
                        _hitstunActive = true;
                        _hitstunTimer = 0f;
                        animationController.SetHitstunActive(_hitstunActive);

                        h.TriggerHitstun(hitstunDuration);
                    }

                    // 3. Try applying knockback
                    if (hit.TryGetComponent(out IKnockable k))
                    {
                        k.TriggerKnockback(transform.forward, knockbackStrength, knockbackDuration);
                    }
                }
            }
        }

        // Update hitstun timer
        if (_hitstunActive)
        {
            _hitstunTimer += deltaTime;
            if (_hitstunTimer >= hitstunDuration)
            {
                _hitstunActive = false;
                animationController.SetHitstunActive(_hitstunActive);
            }
        }
    }

    public void HitboxEnabled(bool b) => _hitboxEnabled = b;

    private void ResetAttack()
    {
        _attackStarted = _attackComplete = _hitboxEnabled = _hitstunActive = false;
        _hitstunTimer = 0f;
        _alreadyHit.Clear();
    }
    #endregion
}
