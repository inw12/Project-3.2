using UnityEngine;
using System.Collections;
using System;
[RequireComponent(typeof(Rigidbody))]
public class Enemy : MonoBehaviour, IDamageable, IKnockable, IHitstunnable
{
    public event Action OnDeath;

    [Header("Enemy Components")]
    [SerializeField] private EnemyHitFeedback hitFeedback;
    [SerializeField] private Animator animator;

    [Header("Stats")]
    [SerializeField] private float maxHealth = 100f;
    private float _currentHealth;

    private Rigidbody _rb;
    private Coroutine _hitstunCoroutine;
    private Coroutine _knockbackCoroutine;

    // Animator Parameters
    private static readonly int KnockbackTrigger = Animator.StringToHash("KnockbackTrigger");
    private static readonly int Hitstun = Animator.StringToHash("InHitstun");

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
        var deltaTime = Time.deltaTime * _timeScale;

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

        Debug.Log($"Current HP: {_currentHealth} / {maxHealth}");

        if (_currentHealth <= 0f)
        {
            OnDeath?.Invoke();
        }
    }
    public void IncreaseHealth(float amount)
    {
        _currentHealth += amount;
        _currentHealth = Mathf.Clamp(_currentHealth, 0f, maxHealth);
    }
    #endregion


    #region *--- 'IKnockable' ----------------------------------------*
    public void TriggerKnockback() => throw new NotImplementedException();

    public void TriggerKnockback(Vector3 direction, float force, float duration)
    {
        // Interupt coroutine if previously running
        if (_knockbackCoroutine != null) StopCoroutine(_knockbackCoroutine);

        // Trigger knockback animation
        animator.SetTrigger(KnockbackTrigger);

        // Rotation enemy towards player
        _rb.MoveRotation(Quaternion.LookRotation(-direction));

        // Start knockback coroutine
        _knockbackCoroutine = StartCoroutine(KnockbackRoutine(direction, force, duration));
    }
    private IEnumerator KnockbackRoutine(Vector3 direction, float force, float duration)
    {
        var elapsed = 0f;

        while (elapsed < duration)
        {
            var deltaTime = Time.deltaTime * _timeScale;

            elapsed += deltaTime;
            var progress = elapsed / duration;

            // Ease out: full force at the start, tapering to zero
            var currentForce = Mathf.Lerp(force, 0f, progress);
            var movement = currentForce * deltaTime * direction;

            _rb.MovePosition(_rb.position + movement);

            yield return null;
        }

        _knockbackCoroutine = null;
    }
    #endregion


    #region *--- 'IHitstunnable' -------------------------------------*
    public float TimeScale => _timeScale;
    public bool InHitstun => _inHitstun;

    public IEnumerator TriggerHitstun(float duration)
    {
        _timeScale = 0f;
        _inHitstun = true;
        animator.SetBool(Hitstun, _inHitstun);
        yield return new WaitForSeconds(duration);
        _timeScale = 1f;
        _inHitstun = false;
        animator.SetBool(Hitstun, _inHitstun);
    }
    #endregion


    #region *--- Public Accessors ----------------------------------------*
    // Set TimeScale
    public void SetTimeScale(float t) => _timeScale = t;
    #endregion


    #region *--- Animator Access ------------------------------*
    public void SetTrigger(string s) => animator.SetTrigger(s);
    #endregion
}
