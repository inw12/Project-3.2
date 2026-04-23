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
    [SerializeField] private float meleeOuterRange = 8f;    // distance from player in which player will "chase" the target when performing melee
    [SerializeField] private float meleeInnerRange = 2.5f;  // distance from player in which to stop moving

    [Header("Hitbox")]
    [SerializeField] private Transform hitboxSpawn;
    [SerializeField] private float hitboxRadius;
    private bool _hitboxEnabled;
    private readonly Collider[] _hits = new Collider[10];   // 'OverlapSphereNonAlloc' buffer
    private readonly HashSet<Collider> _alreadyHit = new(); // records hitbox collisions as they happen (avoids duplicate collision effects)

    [Header("Combo")]
    [SerializeField] private float comboBuffer = 0.4f;
    private const int MaxCombo = 4;
    private int _comboCounter;
    private float _comboTimer;

    // Movement/Rotation during melee
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
        //_dashTimer = 0f;
        _alreadyHit.Clear();
        _rotationTriggered = false;

        // Update Combo Counter
        _comboCounter = _comboCounter == MaxCombo ? 0 : _comboCounter;
        _comboCounter++;
        _comboCounter = Mathf.Clamp(_comboCounter, 0, MaxCombo);

        // Update Animation Controller
        animationController.TriggerMeleeAnimation(_comboCounter);
    }

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

        // Update Player Rotation
        var direction = (state.Target - transform.position).normalized;
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
                
                if (hit.TryGetComponent(out IDamageable e))
                {
                    // 1. Apply Damage
                    e.DecreaseHealth(damage);

                    // 2. Try applying Hitstun

                    // 3. Try applying Knockback
                }
            }
        }
    }

    private void ResetCombo()
    {
        _comboCounter   = 0;
        _comboTimer     = 0f;
        //_dashTimer      = 0f;
    }

    public void HitboxEnabled(bool b) => _hitboxEnabled = b;
}
