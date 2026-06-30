using UnityEngine;
[CreateAssetMenu(menuName = "Enemy Attacks/Ranged/FocusShot")]
public class FocusShot : EnemyRangedAttack
{
    [Header("Laser Stats")]
    [SerializeField] private float laserWidth;


    [Header("Charge Up Info")]
    [SerializeField] private float chargeTime;
    [SerializeField] private float fireDelay;   // amount of time between reaching fully charged and then firing
    private float _chargeTimer;
    private float _delayTimer;

    [Header("Animations")]
    [SerializeField] private AnimationClip chargeAnimation;
    [SerializeField] private AnimationClip delayAnimation;
    private static readonly int PlaybackSpeedA = Animator.StringToHash("PlaybackSpeedA");
    private static readonly int PlaybackSpeedB = Animator.StringToHash("PlaybackSpeedB");

    [SerializeField] private GameObject laserPrefab;
    [SerializeField] private GameObject attackIndicatorPrefab;
    private LaserIndicator _attackIndicator;

    private Vector3 _target;

    public override void Initialize()
    {
        requiresMovement = attackStarted = attackComplete = false;
        _chargeTimer = _delayTimer = 0f;

        _attackIndicator = null;
    }

    public override void Attack(EnemyAttackContext context)
    {
        // Attack START
        if (!attackStarted) 
        {
            attackStarted = true;

            // attack indicator
            var atkInd = Instantiate(attackIndicatorPrefab);
            _attackIndicator = atkInd.GetComponent<LaserIndicator>();
            var indicatorSpawn = context.Enemy.transform.position;
            indicatorSpawn.y = 1.25f;
            _attackIndicator.Initialize(indicatorSpawn, context.PlayerPosition, chargeTime, context.Enemy.transform.position);

            context.AnimationController.SetAttackActive(true);

            // Set playback speed for "charging up"
            float playbackSpeed = chargeAnimation.length / chargeTime;
            context.AnimationController.SetFloat(PlaybackSpeedA, playbackSpeed);

            // Set playback speed for the delay before shooting
            playbackSpeed = delayAnimation.length / fireDelay;
            context.AnimationController.SetFloat(PlaybackSpeedB, playbackSpeed);
        }

        // Update charge timer
        var deltaTime = Time.deltaTime;
        _chargeTimer += deltaTime;

        // Attack UPDATE
        if (!attackComplete)
        {
            // Charged Up!
            if (_chargeTimer >= chargeTime)
            {
                // Fire!
                if (_delayTimer >= fireDelay)
                {
                    Shoot(context);
                    attackComplete = true;
                }

                // update delay timer
                _delayTimer += deltaTime;
            }
            // ONLY track player position WHILE charging
            else
            {
                // update target
                _target = context.PlayerPosition;

                // update character rotation
                var target = _target;
                target.y = 0f;
                var direction = (target - context.Enemy.transform.position).normalized;
                context.Enemy.RotateTowards(direction);

                // update attack indicator
                if (_attackIndicator)
                {
                    _target.y = 1f;
                    _attackIndicator.SetEndPosition(_target);
                }
            }
        }
    }

    private void Shoot(EnemyAttackContext context)
    {
        // Initialize Projectile Stats
        var stats = new LaserStats
        {
            Damage      = damage,
            Speed       = projectileSpeed,
            Range       = range,
            Width       = laserWidth,
            Origin      = context.HitboxSpawn.position,
            Direction   = (_target - context.Enemy.transform.position).normalized,
            TargetLayer = context.PlayerLayer
        };

        var laser = Instantiate(laserPrefab);
        laser.GetComponent<Laser>().Initialize(stats);
    }
}