using UnityEngine;

public struct MeteorStats
{
    public float Damage;
    public float Radius;
    public float Duration;

    public Vector3 Spawn;
    public LayerMask TargetLayer;
    public MeteorPool ObjectPool;

    // Projectile Spawns
    public float ProjectileDamage;
    public ProjectilePool ProjectilePool;
}

public class Meteor : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private AttackIndicator attackIndicator;
    [SerializeField] private int burstProjectileCount;
    [SerializeField] private float burstProjectileSpeed;
    [SerializeField] private float burstProjectileRange;
    private MeteorStats _stats;

    [Header("VFX")]
    [SerializeField] private GameObject lineDrop;

    // hit detection
    private readonly Collider[] _hits = new Collider[5];

    public void Initialize(MeteorStats stats)
    {
        _stats = stats;

        attackIndicator.Initialize(_stats.Duration);
        transform.position = _stats.Spawn;
        transform.localScale = new Vector3(_stats.Radius, _stats.Radius, _stats.Radius);
    }

    void Update()
    {
        attackIndicator.UpdateIndicator();
    }

    private void HandleHit()
    {
        _ = Instantiate(lineDrop, transform.position, Quaternion.identity);

        // Spawn Projectiles
        ProjectileBurst();

        // Scan for hits
        var hits = Physics.OverlapSphereNonAlloc
        (
            transform.position,
            _stats.Radius / 2f,
            _hits,
            _stats.TargetLayer
        );

        // Hit detection
        if (hits > 0)
        {
            var hit = _hits[0];

            if (hit.gameObject.TryGetComponent(out IDamageable i))
            {
                i.DecreaseHealth(_stats.Damage);
            }
        }
        
        // Release from object pool
        _stats.ObjectPool.Release(gameObject);
    }

    void OnEnable()     => attackIndicator.OnComplete += HandleHit;
    void OnDisable()    => attackIndicator.OnComplete -= HandleHit;

    private void ProjectileBurst()
    {
        var angleStep = 360f / burstProjectileCount;
        for (int i = 0; i < burstProjectileCount; i++)
        {
            // Calculate direction
            var angle = i * angleStep;
            var rad = angle * Mathf.Deg2Rad;
            var direction = new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad));
        
            // Initialize projectile stats
            var stats = new ProjectileStats
            {
                Damage = _stats.ProjectileDamage,
                Speed = burstProjectileSpeed,
                Range = burstProjectileRange,
                Direction = direction
            };

            // Fire projectile
            Transform spawn = transform;
            Vector3 spawnPos = transform.position;
            spawnPos.y = 1f;
            spawn.position = spawnPos;
            _stats.ProjectilePool.Get(stats, spawn, _stats.TargetLayer);
        }
    }
}
