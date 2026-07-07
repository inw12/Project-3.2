using UnityEngine;
using System.Collections;
using UnityEngine.Playables;
public class CombatManager : MonoBehaviour
{
    [Header("Main Enemy")]
    [SerializeField] private Enemy enemy;
    [SerializeField] private EnemyCombo enemyCombo;

    [Header("Parry Phase Sequencing")]
    [SerializeField] private Transform enemyPosition;
    [SerializeField] private Transform playerPosition;
    [Space]
    [SerializeField] private float playerPullSpeed;
    [SerializeField] private float playerPullSmooth;

    [Header("Camera")]
    [SerializeField] private CameraManager cameraManager;

    [Header("Parry Phase Cutscene")]
    [SerializeField] private PlayableDirector director;
    [Space]
    [SerializeField] private PlayableAsset enemyComboPlayable;

    [Header("Speed Settings")]
    [SerializeField] [Range(0f, 2f)] private float slowest;
    [SerializeField] [Range(0f, 2f)] private float slow;
    [SerializeField] [Range(0f, 2f)] private float normal;
    [SerializeField] [Range(0f, 2f)] private float fast;
    [SerializeField] [Range(0f, 2f)] private float fastest;

    private bool _triggered;
    private bool _playerPulled;

    void Awake()
    {
        if (enemy)      enemy.OnDeath           += EnterParryPhase;
        if (enemyCombo) enemyCombo.OnComboEnd   += ExitParryPhase;
    
        _triggered = _playerPulled = false;
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
        }

        // "Pulling" the Player
        if (_playerPulled)
        {
            var start       = Player.Instance.transform.position;
            var end         = enemy.transform.position + enemy.transform.forward * 5f; 
            var direction   = (end - start).normalized;
            var next        = Player.Instance.transform.position + (direction * playerPullSpeed);

            Player.Instance.transform.position = Vector3.Lerp
            (
                start,
                next,
                1f - Mathf.Exp(-playerPullSmooth * Time.deltaTime)
            );

            if (Vector3.Distance(Player.Instance.transform.position, end) <= 0.1f) _playerPulled = false;
        }
    }

    // ... more of a "TriggerParryPhase" behavior...
    public void EnterParryPhase()
    {
        // Start sequence
        StartCoroutine(ParryPhaseRoutine());
    }

    /// * Desired sequence:
    ///     1.  Enemy shield activates
    ///     2.  Enemy "Hurt" Animation
    ///     3a. Enemy "Roar" Animation
    ///     3b. Player Input disabled + Player is "pulled" towards Enemy
    ///     4.  Enter Parry Phase
    private IEnumerator ParryPhaseRoutine()
    {
        // 1.
        enemy.EnterParryPhase();
        enemy.EnableShield(true);

        // 2.
        enemy.SetTrigger("KnockbackTrigger");
        yield return null;
        var stateInfo = enemy.GetCurrentAnimationStateInfo(0);
        while (stateInfo.IsName("Knockback_END") && stateInfo.normalizedTime < 1.0f)
        {
            stateInfo = enemy.GetCurrentAnimationStateInfo(0);
            yield return null;
        }
        yield return new WaitForSeconds(1.5f);

        // 3a.
        enemy.transform.rotation = Quaternion.LookRotation(-Vector3.forward);
        enemy.Play("Roar");
        yield return null;
        stateInfo = enemy.GetCurrentAnimationStateInfo(0);
        while (stateInfo.IsName("Roar") && stateInfo.normalizedTime < 1.0f)
        {
            // 3b.
            if (stateInfo.normalizedTime > (112f / 196f) && !_playerPulled) 
            {
                _playerPulled = true;
                Player.Instance.EnterParryPhase();
            }

            stateInfo = enemy.GetCurrentAnimationStateInfo(0);
            yield return null;
        }
        yield return new WaitForSeconds(0.33f);

        // 4.
        if (!_triggered)
        {
            _triggered = true;
            _playerPulled = false;
        }
        Player.Instance.EnterParryPhase();
        enemy.transform.SetPositionAndRotation(enemyPosition.position, enemyPosition.rotation);
        cameraManager.SwitchTo<FocusCamera>();
        director.playableAsset = enemyComboPlayable;
        enemy.EnableShield(false);
        yield return new WaitForSeconds(0.33f);
        director.Play();
    }

    public void ExitParryPhase()
    {
        if (_triggered) _triggered = _playerPulled = false;

        director.Stop();

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

    // playback speed functions
    public void SpeedSlowest()  => director.playableGraph.GetRootPlayable(0).SetSpeed(slowest);
    public void SpeedSlow()     => director.playableGraph.GetRootPlayable(0).SetSpeed(slow);
    public void SpeedNormal()   => director.playableGraph.GetRootPlayable(0).SetSpeed(normal);
    public void SpeedFast()     => director.playableGraph.GetRootPlayable(0).SetSpeed(fast);
    public void SpeedFastest()  => director.playableGraph.GetRootPlayable(0).SetSpeed(fastest);
}
