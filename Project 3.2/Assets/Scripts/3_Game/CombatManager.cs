using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
[RequireComponent(typeof(PlayableDirector))]
public class CombatManager : MonoBehaviour
{
    [Header("Main Enemy")]
    [SerializeField] private Enemy enemy;

    [Header("Parry Phase")]
    [SerializeField] private TimelineAsset parryPhaseSequence;
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
