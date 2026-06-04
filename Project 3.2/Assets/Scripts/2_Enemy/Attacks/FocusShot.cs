using UnityEngine;
[CreateAssetMenu(menuName = "Enemy Attacks/Ranged/FocusShot")]
public class FocusShot : EnemyRangedAttack
{
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

        var deltaTime = Time.deltaTime;

        // Update charge timer
        _chargeTimer += deltaTime;

        if (!attackComplete)
        {
            // Charged Up!
            if (_chargeTimer >= chargeTime)
            {
                _delayTimer += deltaTime;

                // Fire!
                if (_delayTimer >= fireDelay)
                {
                    // update animator
                    context.AnimationController.SetBool("AttackActive", false);

                    // fire projectile
                    FireProjectile(context);

                    // Reset Timers
                    _chargeTimer = _delayTimer = 0f;

                    // Attack END
                    attackComplete = true;
                }
            }
            // Track player position WHILE charging
            else
            {
                _target = context.PlayerPosition;
            }
        }
    }

    private void FireProjectile(EnemyAttackContext context)
    {
        // Initialize Projectile Stats
        var stats = new ProjectileStats
        {
            Damage = damage,
            Speed = projectileSpeed,
            Range = range,
            Direction = (_target - context.Enemy.transform.position).normalized
        };

        // Get Projectile from Object Pool
        context.ProjectilePool.Get(stats, context.HitboxSpawn, context.PlayerLayer);
    }
}