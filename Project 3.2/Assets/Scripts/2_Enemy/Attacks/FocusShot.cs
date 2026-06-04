using UnityEngine;
[CreateAssetMenu(menuName = "Enemy Attacks/Ranged/FocusShot")]
public class FocusShot : EnemyRangedAttack
{
    [Header("Charge Up Info")]
    [SerializeField] private float chargeTime;
    [SerializeField] private float fireDelay;   // amount of time between reaching fully charged and then firing
    private float _chargeTimer;
    private float _delayTimer;

    private Vector3 _target;

    public override void Initialize()
    {
        requiresMovement = attackStarted = attackComplete = false;
        _chargeTimer = _delayTimer = 0f;
    }

    public override void Attack(EnemyAttackContext context)
    {
        if (!attackStarted) attackStarted = true;

        var deltaTime = Time.deltaTime;

        // Update charge timer
        _chargeTimer += deltaTime;

        if (!attackComplete)
        {
            // Attack START
            if (_chargeTimer >= chargeTime)
            {
                // Update delay timer
                _delayTimer += deltaTime;

                // Fire when ready
                if (_delayTimer >= fireDelay)
                {
                    // Initialize Projectile Stats
                    var stats = new ProjectileStats
                    {
                        Damage = damage,
                        Speed = projectileSpeed,
                        Range = range,
                        Direction = _target
                    };

                    // Get Projectile from Object Pool
                    context.ProjectilePool.Get(stats, context.HitboxSpawn, context.PlayerLayer);

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
}
