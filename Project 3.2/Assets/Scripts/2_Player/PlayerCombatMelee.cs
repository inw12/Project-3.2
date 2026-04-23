using UnityEngine;
using System.Collections.Generic;
public class PlayerCombatMelee : MonoBehaviour
{
    public bool ShowDebug;

    [Header("Required Components")]
    [SerializeField] private PlayerAnimationController animationController;
    private LayerMask _targetLayer;

    [Header("Stats")]
    [SerializeField] private float damage = 5f;

    [Header("Movement | Stats")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float acceleration = 20f;
    [SerializeField] private float duration = 0.1f;
    private Vector3 _velocity;
    private float _movementTimer;

    [Header("Movement | Enemy Tracking")]
    [SerializeField] private float meleeOuterRange = 8f;                            // distance from player in which player will "track" the target when performing melee
    [SerializeField] private float meleeInnerRange = 2.5f;                          // distance from player in which to stop "tracking"
    [SerializeField] [Range(1f, 5f)] private float trackingSpeedMultiplier = 1.5f;   // speed multiplier used when "tracking" a target

    [Header("Hitbox")]
    [SerializeField] private Transform hitboxSpawn;
    [SerializeField] private float hitboxRadius;
    private bool _hitboxEnabled;
    private readonly HashSet<Collider> _alreadyHit = new(); // records hitbox collisions as they happen (avoids duplicate collision effects)
    
    [Header("Knockback")]
    [SerializeField] private float knockbackStrength = 10f;
    [SerializeField] private float knockbackDuration = 0.2f;

    [Header("Combo")]
    [SerializeField] private float comboBuffer = 0.4f;
    private const int MaxCombo = 4;
    private int _comboCounter;
    private float _comboTimer;

    // 'OverlapSphereNonAlloc' buffers
    private readonly Collider[] _hits   = new Collider[10];   
    private readonly Collider[] _outer  = new Collider[10];   
    private readonly Collider[] _inner  = new Collider[10];   

    // Movement/Rotation during melee
    private bool _directionCaptured;
    private bool _rotationTriggered;

    void OnDrawGizmos()
    {
        if (!ShowDebug) return;

        if (_hitboxEnabled)
        {
            Gizmos.color = Color.teal;
            Gizmos.DrawWireSphere(hitboxSpawn.position, hitboxRadius);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, meleeOuterRange);
        Gizmos.color = Color.orange;
        Gizmos.DrawWireSphere(transform.position, meleeInnerRange);
    }

    public void Initialize(LayerMask targetLayer)
    {
        ResetCombo();
        _targetLayer = targetLayer;
    }

    public void TriggerAttack()
    {
        // Reset Everything
        _comboTimer = 0f;
        _movementTimer = 0f;

        _alreadyHit.Clear();
        _directionCaptured = false;
        _rotationTriggered = false;

        // Update Combo Counter
        _comboCounter = _comboCounter == MaxCombo ? 0 : _comboCounter;
        _comboCounter++;
        _comboCounter = Mathf.Clamp(_comboCounter, 0, MaxCombo);

        // Update Animation Controller
        animationController.TriggerMeleeAnimation(_comboCounter);
    }

    // Update()
    public void Attack(ref CombatState state, ref bool meleeStarted, ref bool inputEnabled, float deltaTime)
    {
        // Exit Melee State once timer exceeds combo input buffer
        if (_comboTimer > comboBuffer) 
        {
            state.CurrentAction = CombatAction.None;
            meleeStarted = false;
            ResetCombo();
            Player.Instance.MovementInputEnabled(true);
        }

        // Only increment timer while inputs are enabled
        // (inputs are disabled during melee animation and enabled afterwards)
        if (inputEnabled) _comboTimer += deltaTime;
        
        _movementTimer += deltaTime;


        #region *- Target Tracking Implementation ----------*
        // "Where are we moving towards?"
        var outerHits = Physics.OverlapSphereNonAlloc
        (
            transform.position,
            meleeOuterRange,
            _outer,
            _targetLayer
        );
        var innerHits = Physics.OverlapSphereNonAlloc
        (
            transform.position,
            meleeInnerRange,
            _inner,
            _targetLayer
        );
        // * tracking logic goes here *
        #endregion


        // Snapshot target direction
        var direction = transform.forward;
        if (!_directionCaptured)
        {
            _directionCaptured = true;
            direction = (state.Target - transform.position).normalized;
        }

        // Update Velocity
        _velocity = direction * speed;
        _velocity = _movementTimer < duration ? _velocity : Vector3.zero;   // reset velocity if duration is up
        _velocity = innerHits == 0 ? _velocity : Vector3.zero;              // reset velocity if target reached inner range
        Player.Instance.SetVelocity(_velocity, acceleration);

        // Apply Rotation
        if (!_rotationTriggered)
        {
            _rotationTriggered = true;
            Player.Instance.SetRotation(Quaternion.LookRotation(direction));
        }

        UpdateHitbox(deltaTime);
    }

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
                
                // 1. Try applying damage
                if (hit.TryGetComponent(out IDamageable e)) {
                    e.DecreaseHealth(damage);
                }

                // 2. Try applying knockback
                if (hit.TryGetComponent(out IKnockable k))
                {
                    k.TriggerKnockback(transform.forward, knockbackStrength, knockbackDuration);
                }
            }
        }
    }

    private void ResetCombo()
    {
        _comboCounter   = 0;
        _comboTimer     = 0f;
        _movementTimer  = 0f;
    }

    public void HitboxEnabled(bool b) => _hitboxEnabled = b;
}
