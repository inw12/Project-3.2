using UnityEngine;
using System.Collections;
public class CombatManager : MonoBehaviour
{
    [Header("Main Enemy")]
    [SerializeField] private Enemy enemy;
    [SerializeField] private EnemyCombo enemyCombo;

    [Header("Parry Phase Sequencing")]
    [SerializeField] private Transform enemyPosition;
    [SerializeField] private Transform playerPosition;

    [Header("Camera")]
    [SerializeField] private CameraManager cameraManager;

    private bool _triggered;

    private static readonly int Knockback_END    = Animator.StringToHash("Knockback_END");

    void Awake()
    {
        if (enemy)      enemy.OnDeath           += EnterParryPhase;
        if (enemyCombo) enemyCombo.OnComboEnd   += ExitParryPhase;
    
        _triggered = false;
    }

    void OnDestroy()
    {
        if (enemy)      enemy.OnDeath           -= EnterParryPhase;
        if (enemyCombo) enemyCombo.OnComboEnd   -= ExitParryPhase;
    }

    void Update()
    {
        // LOCK THESE MFERS IN PLACE!!!
        if (_triggered)
        {
            Player.Instance.transform.SetPositionAndRotation(
                playerPosition.position,
                playerPosition.rotation
            );
            enemy.transform.SetPositionAndRotation(
                enemyPosition.position,
                enemyPosition.rotation
            );
        }
    }

    // ... more of a "TriggerParryPhase" behavior...
    public void EnterParryPhase()
    {
        // Start sequence
        StartCoroutine(ParryPhaseRoutine());
    }

    /// * Desired sequence:
    ///     1. Enemy shield activates
    ///     2. Enemy animation sequence:
    ///         a. Hurt
    ///         b. "Power Yell"
    ///     3. Screen Shake + Player input disabled
    ///     4. Enemy "Pull" animation
    ///     5. Enter Parry Phase
    private IEnumerator ParryPhaseRoutine()
    {
        // 1.
        enemy.EnterParryPhase();
        enemy.EnableShield(true);

        // 2a.
        enemy.SetTrigger("KnockbackTrigger");
        yield return null;
        var stateInfo = enemy.GetCurrentAnimationStateInfo(0);
        while (stateInfo.IsName("Knockback_END") && stateInfo.normalizedTime < 1.0f)
        {
            stateInfo = enemy.GetCurrentAnimationStateInfo(0);
            yield return null;
        }
        yield return new WaitForSeconds(1f);

        // 2b.
        enemy.Play("Roar");
        yield return null;
        stateInfo = enemy.GetCurrentAnimationStateInfo(0);
        while (stateInfo.IsName("Roar") && stateInfo.normalizedTime < 1.0f)
        {
            // 3.
            if (stateInfo.normalizedTime > (112f / 196f)) Player.Instance.EnterParryPhase();

            stateInfo = enemy.GetCurrentAnimationStateInfo(0);
            yield return null;
        }

        // 4.
        enemy.Play("Pull");
         yield return null;
        stateInfo = enemy.GetCurrentAnimationStateInfo(0);
        while (stateInfo.IsName("Pull") && stateInfo.normalizedTime < 1.0f)
        {
            stateInfo = enemy.GetCurrentAnimationStateInfo(0);
            yield return null;
        }

        // 5.
        if (!_triggered) _triggered = true;
        cameraManager.SwitchTo<FocusCamera>();
        enemy.EnableShield(false);
        yield return new WaitForSeconds(1f);
        enemy.SetTrigger("ComboTrigger");
    }

    public void ExitParryPhase()
    {
        if (_triggered) _triggered = false;

        Player.Instance.ExitParryPhase();

        cameraManager.SwitchTo<CombatCamera>();

        if (enemy.TryGetComponent(out IDamageable e))
        {
            e.IncreaseHealth(float.MaxValue);
        }

        enemy.ExitParryPhase();
    }

    public void PlayPlayerFinisher()
    {
        
    }
}
