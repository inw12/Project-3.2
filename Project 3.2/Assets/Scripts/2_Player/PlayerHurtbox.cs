using UnityEngine;
public class PlayerHurtbox : MonoBehaviour, IDamageable, IKnockable
{
    public bool ShowDebug;
    [Space]

    [SerializeField] private float maxHealth = 100f;
    private float _currentHealth;

    private PlayerAnimationController _animationController;

    void OnGUI()
    {
        if (!ShowDebug) return;
        GUILayout.Label($"Current HP: {_currentHealth} / {maxHealth}");
    }

    public void Initialize(PlayerAnimationController animationController)
    {
        _currentHealth = maxHealth;
        _animationController = animationController;
    }

    #region *--- 'IDamageable' --------------------------------------------------*
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

    #region *--- 'IKnockable' --------------------------------------------------*
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
