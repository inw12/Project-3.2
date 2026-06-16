using UnityEngine;
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerHurtbox : MonoBehaviour, IDamageable, IKnockable
{
    #region * Variables --------------------------------------------------
    [SerializeField] private float maxHealth = 100f;

    private HealthContext _currentHealth;
    private PlayerAnimationController _animationController;
    private CapsuleCollider _hurtbox;
    #endregion


    #region * Initialization --------------------------------------------------
    public void Initialize(PlayerAnimationController animationController)
    {
        _currentHealth = new HealthContext(maxHealth);
        _animationController = animationController;
        _hurtbox = GetComponent<CapsuleCollider>();
    }
    #endregion


    #region * 'IDamageable' Functions --------------------------------------------------
    public HealthContext Health => _currentHealth;

    public void DecreaseHealth(float amount)
    {
        _currentHealth.CurrentHealth -= amount;
        _currentHealth.CurrentHealth = Mathf.Clamp(_currentHealth.CurrentHealth, 0f, maxHealth);
    }
    public void IncreaseHealth(float amount)
    {
        _currentHealth.CurrentHealth += amount;
        _currentHealth.CurrentHealth = Mathf.Clamp(_currentHealth.CurrentHealth, 0f, maxHealth);
    }
    #endregion


    #region * 'IKnockable' Functions --------------------------------------------------
    public void TriggerKnockback()
    {
        _animationController.SetTrigger("HitTrigger");
    }
    public void TriggerKnockback(Vector3 direction, float force, float duration)
    {
        throw new System.NotImplementedException();
    }
    #endregion


    #region * Public Access --------------------------------------------------
    // Returns current health status
    public HealthContext GetHealthStatus() => _currentHealth;

    // Toggles hurtbox capsule on/off
    public void HurtboxEnabled(bool b) => _hurtbox.enabled = b;
    #endregion
}
