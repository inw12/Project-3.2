using UnityEngine;
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerParrybox : MonoBehaviour, IParrybox
{
    #region * Variables --------------------------------------------------
    [SerializeField] private float parryDuration = 0.5f;
    [SerializeField] private float parryCooldown = 1f;
    private float _parryTimer;
    private float _cooldownTimer;

    private CapsuleCollider _parrybox;
    private CapsuleCollider _hurtbox;
    private PlayerAnimationController _animationController;
    #endregion


    #region * Initialization --------------------------------------------------
    public void Initialize(PlayerAnimationController animationController, CapsuleCollider hurtbox)
    {
        _parryTimer = 0f;
        _cooldownTimer = 0f;

        _parrybox = GetComponent<CapsuleCollider>();
        _parrybox.enabled = false;

        _hurtbox = hurtbox;

        _animationController = animationController;
    }
    #endregion


    #region * Update() --------------------------------------------------
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
    #endregion


    #region * 'IParrybox' Functions --------------------------------------------------
    public void TriggerParry()
    {
        _cooldownTimer = parryCooldown;

        // Update Animator
        _animationController.TriggerParry();

        // * parry effect implementation here *
    }
    #endregion


    #region * Public Access
    // Returns true/false if another parry can be inputted
    public bool CanParry() => _cooldownTimer > parryCooldown;

    // Toggles parrybox on/off
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
    #endregion
}
