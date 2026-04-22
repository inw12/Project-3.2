using UnityEngine;
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

    private PlayerAnimationController _animationController;
    private Vector3 _hitboxSpawn;
    private float _hitboxRadius;

    public void Initialize(PlayerAnimationController animator, Transform meleeHitbox, float hitboxRadius)
    {
        ResetCombo();

        _animationController = animator;

        _hitboxSpawn = meleeHitbox.position;
        _hitboxRadius = hitboxRadius;
    }

    public void TriggerAttack()
    {
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

    public void Attack()
    {
        
    }

    private void ResetCombo()
    {
        _comboCounter   = 0;
        _comboTimer     = 0f;
        //_dashTimer      = 0f;
    }
}
