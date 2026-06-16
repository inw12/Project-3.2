using UnityEngine;
public class PlayerHurtbox : MonoBehaviour, IDamageable, IKnockable
{
    #region * Variables --------------------------------------------------
    [SerializeField] private float maxHealth = 100f;
    private float _currentHealth;

    private PlayerAnimationController _animationController;
    #endregion


    #region * Initialization --------------------------------------------------
    public void Initialize(PlayerAnimationController animationController)
    {
        _currentHealth = maxHealth;
        _animationController = animationController;
    }
    #endregion


    #region * 'IDamageable' Functions --------------------------------------------------
    public float MaxHealth => maxHealth;
    public float CurrentHealth => _currentHealth;

    public void DecreaseHealth(float amount)
    {
        _currentHealth -= amount;
        _currentHealth = Mathf.Clamp(_currentHealth, 0f, maxHealth);
    }
    public void IncreaseHealth(float amount)
    {
        _currentHealth += amount;
        _currentHealth = Mathf.Clamp(_currentHealth, 0f, maxHealth);
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
}
