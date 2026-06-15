using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class PlayerCombatMelee : MonoBehaviour
{
    public bool ShowDebug;

    #region * Variables --------------------------------------------------
    [Header("Stats")]
    [SerializeField] private float damage = 5f;
    [Space]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float acceleration = 20f;
    [SerializeField] private float duration = 0.1f;

    [Header("Animation")]
    [SerializeField] private PlayerAnimationController animationController;
    [SerializeField] private AnimationClip meleeStartClip;
    [SerializeField] private AnimationClip meleeActiveClip;
    [SerializeField] private AnimationClip meleeEndClip;
    private static readonly int MeleeTrigger = Animator.StringToHash("MeleeTrigger");
    private static readonly int MeleePhase = Animator.StringToHash("MeleePhase");

    [Header("Frame Data")]
    [SerializeField] private int startupFrames = 14;
    [SerializeField] private int activeFrames = 3;
    [SerializeField] private int endlagFrames = 20;
    private float StartDuration  => (float)startupFrames   / _frameRate;
    private float ActiveDuration => (float)activeFrames    / _frameRate;
    private float EndlagDuration => (float)endlagFrames    / _frameRate;
    private readonly int _frameRate = 60;

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

    // Action trackers
    private bool _attackStarted;
    private bool _attackComplete;
    #endregion

    #region * Debugging --------------------------------------------------
    // Draw hitbox when active
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

    #region * Initialization
    public void Initialize(LayerMask targetLayer)
    {
        _targetLayer = targetLayer;
        ResetAttack();
    }
    #endregion

    #region * Attack Implementation ---------------------------------------
    // Called by 'PlayerCombat.cs' in 'OnMeleeAttack()' (called every frame while in "Melee" state)
    public void Attack(ref CombatState state)
    {
        // START
        if (!_attackStarted) AttackStart(ref state);

        // END
        if (_attackComplete) AttackEnd(ref state);
        
        UpdateHitbox();
    }

    // Attack START
    private void AttackStart(ref CombatState state)
    {
        _attackStarted = true;

        // 1. Disable Player input (for the duration of this attack)
        Player.Instance.InputEnabled(false);

        // 2. Rotate character towards cursor
        RotateTowards(state.Target);

        // 3. Start Melee Coroutine
        StartCoroutine(MeleeAttack());
    }

    // Attack ACTIVE
    private IEnumerator MeleeAttack()
    {
        animationController.SetTrigger(MeleeTrigger);

        // * Start -------------------------------
        SetPhase(1);
        SetClipSpeed(meleeStartClip, startupFrames);
        yield return new WaitForSeconds(StartDuration);

        // * Active ------------------------------
        SetPhase(2);
        SetClipSpeed(meleeActiveClip, activeFrames);
        _hitboxEnabled = true;
        yield return new WaitForSeconds(ActiveDuration);
        _hitboxEnabled = false;

        // * End ---------------------------------
        SetPhase(3);
        SetClipSpeed(meleeEndClip, endlagFrames);
        yield return new WaitForSeconds(EndlagDuration);

        // * Reset -------------------------------
        SetPhase(0);
        ResetAnimatorSpeed();
        _attackComplete = true;
    }

    // Attack END
    private void AttackEnd(ref CombatState state)
    {
        // 1. Re-enable Player input
        Player.Instance.InputEnabled(true);

        // 2. Exit from 'Melee' state
        state.CurrentAction = CombatAction.None;

        // 3. Reset ALL variables
        ResetAttack();
    }
    #endregion
    
    #region * Helper Functions --------------------------------------------------

    // Rotate towards cursor
    private void RotateTowards(Vector3 target)
    {
        // Snapshot target direction
        var direction = (target - transform.position).normalized;

        // Apply rotation
        Player.Instance.SetRotation(Quaternion.LookRotation(direction));
    }

    // Sets Melee animation phase
    private void SetPhase(int i) 
    {
        animationController.SetInteger(MeleePhase, i);
    }

    // Sets animator speed to target speed of current clip
    private void SetClipSpeed(AnimationClip clip, int targetFrames)
    {
        if (clip == null) return;

        var targetDuration = (float)targetFrames / _frameRate;

        // Speed = ClipDuration / DesiredDuration
        var targetSpeed = clip.length / targetDuration;

        animationController.SetSpeed(targetSpeed);
    }

    // Sets animator speed back to 1
    private void ResetAnimatorSpeed()
    {
        animationController.SetSpeed(1);
    }

    // Resets ALL variables
    private void ResetAttack()
    {
        _attackStarted = _attackComplete = _hitstunActive = false;
        _hitboxEnabled = false;
        _hitstunTimer = 0f;
        _alreadyHit.Clear();
    }

    // When active, searches for "IDamageable" objects in range
    private void UpdateHitbox()
    {
        // Search for hits
        if (_hitboxEnabled)
        {
            var hits = Physics.OverlapSphereNonAlloc
            (
                hitboxSpawn.position,
                hitboxRadius,
                _hits,
                _targetLayer
            );

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
            _hitstunTimer += Time.deltaTime;
            if (_hitstunTimer >= hitstunDuration)
            {
                _hitstunActive = false;
                animationController.SetHitstunActive(_hitstunActive);
            }
        }
    }
    #endregion
}
