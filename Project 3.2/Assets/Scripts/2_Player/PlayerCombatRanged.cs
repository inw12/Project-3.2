using UnityEngine;
[RequireComponent(typeof(ProjectilePool))]
public class PlayerCombatRanged : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float damage = 1f;
    [SerializeField] private float fireRate = 0.4f;
    [SerializeField] private float projectileSpeed = 40f;
    [SerializeField] private float projectileRange = 75f;
    [Space]
    [SerializeField] private Transform projectileSpawn;

    private ProjectilePool _pool;
    private LayerMask _targetLayer;
    private float _fireTimer;

    // Start()
    public void Initialize(LayerMask targetLayer)
    {
        _pool = GetComponent<ProjectilePool>();
        _targetLayer = targetLayer;
        _fireTimer = fireRate;
    }

    // Update()
    public void Attack(ref CombatState state, float deltaTime)
    {
        // Increment Fire Rate Timer
        _fireTimer += deltaTime;

        // Calculate Projectile Direction
        var source = Vector3.ProjectOnPlane(projectileSpawn.position, Vector3.up);
        var direction = (state.Target - source).normalized;

        // Fire Projectile
        if (_fireTimer >= fireRate)
        {
            var stats = new ProjectileStats
            {
                Damage = damage,
                Speed = projectileSpeed,
                Range = projectileRange,
                Direction = direction
            };
            _pool.Get(stats, projectileSpawn, _targetLayer);

            _fireTimer = 0f;
        }
    }
}
