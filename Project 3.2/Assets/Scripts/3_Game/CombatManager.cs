using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
[RequireComponent(typeof(PlayableDirector))]
public class CombatManager : MonoBehaviour
{
    [Header("Main Enemy")]
    [SerializeField] private Enemy enemy;

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
    }

    public void EnterParryPhase()
    {
        Player.Instance.InputEnabled(false);
        Player.Instance.ParryInputEnabled(true);

        Player.Instance.transform.SetPositionAndRotation(
            playerPosition.position,
            playerPosition.rotation
        );
        enemy.transform.SetPositionAndRotation(
            enemyPosition.position,
            enemyPosition.rotation
        );

        cameraManager.SwitchTo<FocusCamera>();
    }

    public void ExitParryPhase()
    {
        
    }

    public void PlayPlayerFinisher()
    {
        
    }

    void OnDestroy()
    {
        if (enemy) enemy.OnDeath -= EnterParryPhase;
    }
}
