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

    [Header("Number of Projectiles per Attack Instance")]
    public int shotMin;
    public int shotMax;

    private float _fireTimer;
    private float _durationTimer;
    private int _shotCount;

    // keeping track of attack duration
    private float _duration;
    private bool _attackTriggered;
    [HideInInspector] public bool attackComplete;

    // Called every frame
    public override void Attack(EnemyAttackContext context)
    {
        // Initialize attack
        if (!_attackTriggered)
        {
            _attackTriggered = true;
            attackComplete = false;
            _duration = Random.Range(durationMin, durationMax);
        }

        var deltaTime = Time.deltaTime;

        _durationTimer += deltaTime;
        _fireTimer += deltaTime;

        // End attack if duration is up
        if (_durationTimer >= _duration)
        {
            attackComplete = true;
            _attackTriggered = false;
        }

        // Trigger attack if attack not complete
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

            _shotCount++;
        }
    }
}
