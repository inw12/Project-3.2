using UnityEngine;
[CreateAssetMenu(fileName = "RangedAttack", menuName = "Player Attacks/Ranged")]
public class RangedAttack : ScriptableObject
{
    [Header("Stats")]
    [SerializeField] private float damage = 1f;
    [SerializeField] private float fireRate = 0.4f;
    [SerializeField] private float projectileSpeed = 40f;
    [SerializeField] private float projectileRange = 75f;
    private float _fireTimer;
    private Vector3 _projectileDirection;

    private ProjectilePool _pool;
    private Transform _projectileSpawn;

    public void Initialize(ProjectilePool pool, Transform spawn)
    {
        _pool = pool;
        _projectileSpawn = spawn;

        _fireTimer = fireRate;
    }

    // Update()
    public void Attack(ref CombatState state, float deltaTime)
    {
        // Increment Fire Rate Timer
        _fireTimer += deltaTime;

        // Calculate Projectile Direction
        var source = Vector3.ProjectOnPlane(_projectileSpawn.position, Vector3.up);
        _projectileDirection = (state.Target - source).normalized;

        // Fire Projectile
        if (_fireTimer >= fireRate)
        {
            var stats = new ProjectileStats
            {
                Damage = damage,
                Speed = projectileSpeed,
                Range = projectileRange,
                Direction = _projectileDirection
            };
            _pool.Get(stats, _projectileSpawn);

            _fireTimer = 0f;
        }
    }
}
