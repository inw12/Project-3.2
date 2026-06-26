using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class Enemy : MonoBehaviour, IDamageable, IKnockable, IHitstunnable
{
    #region * Variables
    [Header("Unity Components")]
    [SerializeField] private EnemyAI enemyAI;
    [SerializeField] private EnemyHitFeedback hitFeedback;
    [Space]
    [SerializeField] private EnemyAnimationController animationController;
    [SerializeField] private Animator animator;
    [SerializeField] private EnemyArmRig armRig;

    [Header("Stats")]
    [SerializeField] private float maxHealth = 100f;
    private HealthContext _currentHealth;

    private Rigidbody _rb;
    public event Action OnDeath;

    // Animator Parameters
    private static readonly int KnockbackTrigger = Animator.StringToHash("KnockbackTrigger");
    private static readonly int Hitstun = Animator.StringToHash("InHitstun");

    // Hitstun & Knockback
    private float _timeScale;
    private bool _inHitstun;
    private Coroutine _hitstunCoroutine;
    private Coroutine _knockbackCoroutine;
    #endregion

    #region * Initialization
    void Start()
    {
        animationController.Initialize();
        armRig.Initialize();
        enemyAI.Initialize(this);
        hitFeedback.Initialize();

        _rb = GetComponent<Rigidbody>();

        _currentHealth = new HealthContext(maxHealth);
        _timeScale = 1f;
        _inHitstun = false;
    }
    #endregion

    #region * Update Functions
    void Update()
    {
        var deltaTime = Time.deltaTime * _timeScale;

        // State Machine logic
        enemyAI.UpdateAI(deltaTime);
    }
    void LateUpdate()
    {
        var deltaTime = Time.deltaTime * _timeScale;

        // State Machine current action
        enemyAI.LateUpdateAI(deltaTime);

        // Animation Parameters based on Enemy State
        var state = enemyAI.GetState();
        var animationContext = new EnemyAnimatorContext
        {
            Velocity        = state.Velocity,
            CurrentAction   = (int)state.CurrentAction,
            AttackID        = state.CurrentAttack != null ? state.CurrentAttack.attackID : -1,
            InHitstun       = _inHitstun
        };
        animationController.UpdateAnimator(animationContext);

        // Arm rig
        armRig.UpdateRig();

        // Hit Feedback when taking damage
        if (hitFeedback) hitFeedback.UpdateEnemyModel(deltaTime);
    }
    void FixedUpdate()
    {
        var fixedDeltaTime = Time.fixedDeltaTime * _timeScale;

        // Movement from State Machine
        enemyAI.UpdateMovement(fixedDeltaTime);
    }
    #endregion

    #region * 'IDamageable'
    public HealthContext Health => _currentHealth;

    public void DecreaseHealth(float amount)
    {
        _currentHealth.CurrentHealth -= amount;
        _currentHealth.CurrentHealth = Mathf.Clamp(_currentHealth.CurrentHealth, 0f, maxHealth);

        if (hitFeedback) hitFeedback.TriggerHitFeedback();

        if (_currentHealth.CurrentHealth <= 0f)
        {
            OnDeath?.Invoke();
        }
    }
    public void IncreaseHealth(float amount)
    {
        _currentHealth.CurrentHealth += amount;
        _currentHealth.CurrentHealth = Mathf.Clamp(_currentHealth.CurrentHealth, 0f, maxHealth);
    }
    #endregion

    #region * 'IKnockable'
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

    #region * 'IHitstunnable' 
    public float TimeScale => _timeScale;
    public bool InHitstun => _inHitstun;

    public void TriggerHitstun(float duration)
    {
        // Interupt coroutine if previously running
        if (_hitstunCoroutine != null) StopCoroutine(_hitstunCoroutine);

        // Start Coroutine
        _hitstunCoroutine = StartCoroutine(HitstunCoroutine(duration));
    }
    private IEnumerator HitstunCoroutine(float duration)
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

    #region * Gateway Functions
    // Set TimeScale
    public void SetTimeScale(float t) => _timeScale = t;

    // Set to Idle
    public void SetToIdle() 
    {
        enemyAI.SetToIdle();
        animationController.SetToIdle();
        ArmRigEnabled(false);
    }
    
    // Toggle Enemy State Machine
    public void EnemyActive(bool b) => enemyAI.EnemyActive(b);

    // Access EnemyAI Movement
    public void SetMovementTarget(Vector3 position) => enemyAI.SetMovementTarget(position);

    // Character rotation
    public void RotateTowards(Vector3 direction) => enemyAI.RotateTowards(direction);

    // EnemyAI State Getter
    public EnemyState GetState() => enemyAI.GetState();

    // Player Position Getter
    public Vector3 GetPlayerPosition() => enemyAI.GetPlayerPosition();

    // Arm Rig Toggle
    public void ArmRigEnabled(bool b) => armRig.ArmRigEnabled(b);

    // Animator Access
    public void SetInteger(string s, int i) => animationController.SetInteger(s, i);
    public int GetInteger(string s)         => animationController.GetInteger(s);
    public void SetFloat(string s, int i)   => animationController.SetFloat(s, i);
    public float GetFloat(string s)         => animationController.GetFloat(s);
    public void SetBool(string s, bool b)   => animationController.SetBool(s, b);
    public bool GetBool(string s)           => animationController.GetBool(s);
    public void SetTrigger(string s)        => animationController.SetTrigger(s);
    #endregion
}