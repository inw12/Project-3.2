using UnityEngine;
using System;
public class EnemyHurtbox : MonoBehaviour, IDamageable
{
    [Header("Effects")]
    [SerializeField] private EnemyHitFeedback hitFeedback;

    // Health Variables
    private float _maxHealth;
    private float _currentHealth;
    public float MaxHealth => _maxHealth;
    public float CurrentHealth => _currentHealth;

    // Death event signal
    public event Action DeathTriggered;

    public void Initialize(float maxHealth)
    {
        _maxHealth = maxHealth;
        _currentHealth = _maxHealth;

        hitFeedback?.Initialize();
    }

    // LateUpdate()
    public void UpdateHurtbox(float deltaTime)
    {
        hitFeedback.UpdateEnemyModel(deltaTime);
    }

    #region *--- 'IDamageable' Methods ------------------------------*
    public void DecreaseHealth(float amount)
    {
        _currentHealth -= amount;
        _currentHealth = Mathf.Clamp(_currentHealth, 0f, _maxHealth);

        Debug.Log($"Current HP: {_currentHealth}");

        hitFeedback.TriggerHitFeedback();

        // Check for death
        if (_currentHealth <= 0f)
        {
            DeathTriggered?.Invoke();
        }
    }
    public void IncreaseHealth(float amount)
    {
        _currentHealth += amount;
        _currentHealth = Mathf.Clamp(_currentHealth, 0f, _maxHealth);
    }
    #endregion
}
