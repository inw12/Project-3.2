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
        if (_triggered)
        {
            // LOCK THESE MFERS IN PLACE!!!
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

    public void EnterParryPhase()
    {
        if (!_triggered) _triggered = true;

        // Reset Player
        Player.Instance.EnterParryPhase();

        // Reset Enemy
        enemy.EnterParryPhase();

        // Start sequence
        StartCoroutine(ParryPhaseRoutine());
    }
    private IEnumerator ParryPhaseRoutine()
    {
        enemy.EnableShield(false);
        
        cameraManager.SwitchTo<FocusCamera>();
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
