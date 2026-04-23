using UnityEngine;
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerParrybox : MonoBehaviour
{
    [SerializeField] private float parryDuration = 0.5f;
    private float _parryTimer;

    private CapsuleCollider _parrybox;
    private CapsuleCollider _hurtbox;
    private PlayerAnimationController _animationController;

    // Start()
    public void Initialize(PlayerAnimationController animationController, CapsuleCollider hurtbox)
    {
        _parryTimer = 0f;

        _parrybox = GetComponent<CapsuleCollider>();
        _parrybox.enabled = false;

        _hurtbox = hurtbox;

        _animationController = animationController;
    }

    public void UpdateParrybox(ref bool parryStarted, float deltaTime)
    {
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
    public void ParryboxEnabled(bool active)
    {
        _parrybox.enabled = active;
        _hurtbox.enabled = !active;
    }
    public void TriggerParry()
    {
        // Update Animator
        _animationController.TriggerParry();

        // * parry effect implementation here *
    }
    #endregion
}
