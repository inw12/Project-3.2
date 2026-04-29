using UnityEngine;
public class PlayerHurtbox : MonoBehaviour, IDamageable
{
    public bool ShowDebug;
    [Space]

    [SerializeField] private float maxHealth = 100f;
    private float _currentHealth;

    void OnGUI()
    {
        if (!ShowDebug) return;
        GUILayout.Label($"Current HP: {_currentHealth} / {maxHealth}");
    }

    public void Initialize()
    {
        _currentHealth = maxHealth;
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
}
