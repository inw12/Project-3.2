using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(fileName = "MeleeAttack", menuName = "Player Attacks/Melee")]
public class MeleeAttack : ScriptableObject
{
    [Header("Stats")]
    [SerializeField] private float damage = 5f;
    [SerializeField] private float meleeOuterRange = 8f;    // distance from player in which player will "chase" the target when performing melee
    [SerializeField] private float meleeInnerRange = 2.5f;  // distance from player in which to stop moving

    [Header("Combo")]
    [SerializeField] private float comboBuffer = 0.4f;
    private const int MaxCombo = 4;
    private int _comboCounter;
    private float _comboTimer;

    [SerializeField] private LayerMask targetLayer;

    private PlayerAnimationController _animationController;

    // Hitbox variables
    private Vector3 _hitboxSpawn;
    private float _hitboxRadius;
    private bool _hitboxEnabled;
    private readonly Collider[] _hits = new Collider[10];   // 'OverlapSphereNonAlloc' buffer
    private readonly HashSet<Collider> _alreadyHit = new(); // records hitbox collisions as they happen (avoids duplicate collision effects)

    public void Initialize(PlayerAnimationController animator, Transform meleeHitbox, float hitboxRadius)
    {
        ResetCombo();

        _animationController = animator;

        _hitboxSpawn = meleeHitbox.position;
        _hitboxRadius = hitboxRadius;
    }

    public void TriggerAttack()
    {
        // Clear recorded collisions
        _alreadyHit.Clear();

        // Reset Timers
        _comboTimer = 0f;
        //_dashTimer = 0f;

        // Update Combo Counter
        _comboCounter = _comboCounter == MaxCombo ? 0 : _comboCounter;
        _comboCounter++;
        _comboCounter = Mathf.Clamp(_comboCounter, 0, MaxCombo);

        // Update Animation Controller
        _animationController.TriggerMeleeAnimation(_comboCounter);
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

        // Input buffers should only update when able to input
        if (inputEnabled)
        {
            // Increment Timers
            _comboTimer += deltaTime;
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
                _hitboxSpawn,
                _hitboxRadius,
                _hits,
                targetLayer
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
