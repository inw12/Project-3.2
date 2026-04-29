using System;
using System.Collections;
using Codice.CM.Common.Serialization;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
[RequireComponent(typeof(PlayableDirector))]
public class CombatManager : MonoBehaviour
{
    [Header("Main Enemy")]
    [SerializeField] private Enemy enemy;
    [SerializeField] private EnemyCombo enemyCombo;

    [Header("Parry Phase Sequencing")]
    [SerializeField] private TimelineAsset parryPhaseSequence;
    [SerializeField] private Transform enemyPosition;
    [SerializeField] private Transform playerPosition;
    private PlayableDirector _director;

    [Header("Camera")]
    [SerializeField] private CameraManager cameraManager;

    void Awake()
    {
        _director = GetComponent<PlayableDirector>();
        if (enemy) enemy.OnDeath += EnterParryPhase;
        if (enemyCombo) enemyCombo.OnComboEnd += ExitParryPhase;
    }

    public void EnterParryPhase()
    {
        Player.Instance.SetToIdle();
        Player.Instance.InputEnabled(false);
        Player.Instance.ParryInputEnabled(true);
        Player.Instance.SetBoolean("InParryPhase", true);

        Player.Instance.transform.SetPositionAndRotation(
            playerPosition.position,
            playerPosition.rotation
        );
        enemy.transform.SetPositionAndRotation(
            enemyPosition.position,
            enemyPosition.rotation
        );

        cameraManager.SwitchTo<FocusCamera>();

        StartCoroutine(ParryPhaseRoutine());
    }
    private IEnumerator ParryPhaseRoutine()
    {
        yield return new WaitForSeconds(1f);
        enemy.SetTrigger("ComboTrigger");
    }

    public void ExitParryPhase()
    {
        Player.Instance.InputEnabled(true);
        Player.Instance.SetBoolean("InParryPhase", false);
        cameraManager.SwitchTo<DefaultCamera>();

        if (enemy.TryGetComponent(out IDamageable e))
        {
            e.IncreaseHealth(float.MaxValue);
        }
    }

    public void PlayPlayerFinisher()
    {
        
    }

    void OnDestroy()
    {
        if (enemy) enemy.OnDeath -= EnterParryPhase;
        if (enemyCombo) enemyCombo.OnComboEnd -= ExitParryPhase;
    }
}
