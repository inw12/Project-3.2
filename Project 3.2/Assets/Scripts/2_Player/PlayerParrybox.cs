using UnityEngine;
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerParrybox : MonoBehaviour, IParrybox
{
    [SerializeField] private float parryDuration = 0.5f;
    [SerializeField] private float parryCooldown = 1f;
    private float _parryTimer;
    private float _cooldownTimer;

    private CapsuleCollider _parrybox;
    private CapsuleCollider _hurtbox;
    private PlayerAnimationController _animationController;

    // Start()
    public void Initialize(PlayerAnimationController animationController, CapsuleCollider hurtbox)
    {
        _parryTimer = 0f;
        _cooldownTimer = 0f;

        _parrybox = GetComponent<CapsuleCollider>();
        _parrybox.enabled = false;

        _hurtbox = hurtbox;

        _animationController = animationController;
    }

    public void UpdateParrybox(ref bool parryStarted, float deltaTime)
    {
        _cooldownTimer += deltaTime;

        // Start counting when parrybox is active
        if (_parrybox.enabled)
        {
            _parryTimer += deltaTime;

            if (_parryTimer > parryDuration)
            {
                _parryTimer = 0f;
                ParryboxEnabled(false);
                parryStarted = false;
            }
        }
    }

    #region *--- Public Methods to Enable/Disable/Trigger Parry ----------*
    public bool CanParry() => _cooldownTimer > parryCooldown;
    public void ParryboxEnabled(bool active)
    {
        // Parrybox ON
        if (active)
        {
            _parrybox.enabled = true;
            _hurtbox.enabled = false;
            _cooldownTimer = 0f;
        }
        // Parrybox OFF
        else
        {
            _parrybox.enabled = false;
            _hurtbox.enabled = true;
        }
    }
    public void TriggerParry()
    {
        _cooldownTimer = parryCooldown;

        // Update Animator
        _animationController.TriggerParry();

        // * parry effect implementation here *
    }
    #endregion
}
