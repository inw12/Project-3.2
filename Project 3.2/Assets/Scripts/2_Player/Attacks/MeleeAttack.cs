using UnityEngine;
[CreateAssetMenu(fileName = "MeleeAttack", menuName = "Player Attacks/Melee")]
public class MeleeAttack : ScriptableObject
{
    [Header("Stats")]
    [SerializeField] private float damage = 5f;
    [SerializeField] private float meleeOuterRange = 8f;    // distance from player in which player will "chase" the target when performing melee
    [SerializeField] private float meleeInnerRange = 2.5f;  // distance from player in which to stop moving

    [Header("Dash Movement")]
    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashAcceleration;
    [SerializeField] private float dashDuration;
    [SerializeField] [Range(0f, 5f)] private float targetedDashSpeedMultiplier;
    private Vector3 _dashVelocity;
    private float _dashTimer;

    [Header("Combo")]
    [SerializeField] private float comboBuffer = 0.4f;
    private const int MaxCombo = 4;
    private int _comboCounter;
    private float _comboTimer;

    [Header("Hitstun")]
    [SerializeField] private float hitstunDuration = 0.125f;
    private bool _hitstunActive;
    private float _hitstunTimer;

    private PlayerAnimationController _animationController;
    private CapsuleCollider _hitbox;

    public void Initialize(PlayerAnimationController animator, CapsuleCollider hitbox)
    {
        ResetCombo();

        _animationController = animator;
        _hitbox = hitbox;
        _hitbox.enabled = false;
    }

    public void TriggerAttack()
    {
        // Reset Timers
        _comboTimer = 0f;
        _dashTimer = 0f;

        // Update Combo Counter
        _comboCounter = _comboCounter == MaxCombo ? 0 : _comboCounter;
        _comboCounter++;
        _comboCounter = Mathf.Clamp(_comboCounter, 0, MaxCombo);

        // Update Animation Controller
        _animationController.TriggerMeleeAnimation(_comboCounter);
    }

    public void Attack()
    {
        
    }

    private void ResetCombo()
    {
        _comboCounter   = 0;
        _comboTimer     = 0f;
        _dashTimer      = 0f;
    }
}
