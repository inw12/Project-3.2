using UnityEngine;
[CreateAssetMenu(menuName = "Enemy Attacks/Ranged/ScatterShot")]
public class ScatterShot : EnemyRangedAttack
{
    [Header("Stats")]
    public float damage;
    public float fireRate;
    public float projectileSpeed;
    public float range;

    [Header("Duration")]
    public float durationMin;
    public float durationMax;
    private float _duration;

    [Header("Number of Projectiles per Attack Instance")]
    public int shotMin;
    public int shotMax;

    // Timers & Counters
    private float _fireTimer;
    private float _durationTimer;
    private int _shotCount;

    public override void Initialize()
    {
        // Does this attack require movement?
        requiresMovement = false;

        // Reset timers + counters
        _fireTimer = fireRate;
        _durationTimer = 0f;
        _shotCount = 0;

        // Reset logic checks
        attackStarted = false;
        attackComplete = false;
    }

    // Called in 'Update()' in EnemyAI.cs
    public override void Attack(EnemyAttackContext context)
    {
        // Attack START
        if (!attackStarted)
        {
            attackStarted = true;
            _duration = Random.Range(durationMin, durationMax);
        }

        // Update timers
        var deltaTime = Time.deltaTime;
        _durationTimer += deltaTime;
        _fireTimer += deltaTime;

        // Attack END
        attackComplete = _durationTimer >= _duration;

        // Attack Implementation
        if (_fireTimer >= fireRate && !attackComplete)
        {
            var amount = Random.Range(shotMin, shotMax + 1);
            for (int i = 0; i < amount; i++)
            {
                // Get Random Direction
                Vector2 randomCircle = Random.insideUnitCircle;
                Vector3 randomPoint = new(randomCircle.x, 0f, randomCircle.y);
                randomPoint = randomPoint.normalized;

                // Initialize Projectile Stats
                var stats = new ProjectileStats
                {
                    Damage = damage,
                    Speed = projectileSpeed,
                    Range = range,
                    Direction = randomPoint
                };

                // Get Projectile from Object Pool
                context.ProjectilePool.Get(stats, context.HitboxSpawn, context.PlayerLayer);
            }

            _fireTimer = 0f;
            _shotCount++;
        }
    }
}
