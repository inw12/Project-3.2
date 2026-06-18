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

    private Vector3 _target;

    public override void Initialize()
    {
        requiresMovement = attackStarted = attackComplete = false;
        _chargeTimer = _delayTimer = 0f;
    }

    public override void Attack(EnemyAttackContext context)
    {
        // Attack START
        if (!attackStarted) 
        {
            attackStarted = true;

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