using UnityEngine;
using System.Collections;
[RequireComponent(typeof(Rigidbody))]
public class Enemy : MonoBehaviour, IDamageable, IKnockable, IHitstunnable
{
    [Header("Enemy Components")]
    [SerializeField] private EnemyHitFeedback hitFeedback;

    [Header("Stats")]
    [SerializeField] private float maxHealth = 100f;
    private float _currentHealth;

    private Rigidbody _rb;

    // Hitstun 
    private float _timeScale;
    private bool _inHitstun;

    void Start()
    {
        // Initialize Components
        hitFeedback.Initialize();
        _rb = GetComponent<Rigidbody>();

        // Initialize Variables
        _currentHealth = maxHealth;

        _timeScale = 1f;
        _inHitstun = false;
    }

    void Update()
    {
        
    }

    void LateUpdate()
    {
        var deltaTime = Time.deltaTime;

        if (hitFeedback) hitFeedback.UpdateEnemyModel(deltaTime);
    }

    #region *--- 'IDamageable' --------------------------------------------------*
    public float MaxHealth => maxHealth;
    public float CurrentHealth => _currentHealth;

    public void DecreaseHealth(float amount)
    {
        _currentHealth -= amount;
        _currentHealth = Mathf.Clamp(_currentHealth, 0f, maxHealth);

        if (hitFeedback) hitFeedback.TriggerHitFeedback();
    }
    public void IncreaseHealth(float amount)
    {
        _currentHealth += amount;
        _currentHealth = Mathf.Clamp(_currentHealth, 0f, maxHealth);
    }
    #endregion


    #region *--- 'IKnockable' ----------------------------------------*
    public void TriggerKnockback(Vector3 direction, float amount)
    {
        throw new System.NotImplementedException();
    }
    #endregion


    #region *--- 'IHitstunnable' -------------------------------------*
    public float TimeScale => _timeScale;
    public bool InHitstun => _inHitstun;

    public IEnumerator TriggerHitstun(float duration)
    {
        throw new System.NotImplementedException();
    }
    #endregion

}
