using UnityEngine;
[CreateAssetMenu(menuName = "Enemy Attacks/Area/MeteorShower")]
public class MeteorShower : EnemyAreaAttack
{
    [Space]
    [SerializeField] private MeteorSpawner meteorSpawner;

    [Header("Meteor Stats")]
    [SerializeField] private float radius;          // size of attack indicator
    [SerializeField] private float chargeTime;      // how long until indicator is filled?
    [SerializeField] private LayerMask targetMask;  // what are we hitting?

    [Header("Spawner Stats")]
    [SerializeField] private float SpawnerRadius;
    [SerializeField] private int SpawnAmount;
    [SerializeField] private float SpawnCooldown;

    private MeteorSpawner _spawner;

    public override void Initialize()
    {
        _attackShape = AreaAttackShape.Circle;
        requiresMovement = false;

        attackStarted = attackComplete = false;
    }

    public override void Attack(EnemyAttackContext context)
    {
        if (!attackStarted)
        {
            attackStarted = true;

            // Spawn the spawner
            _spawner = Instantiate(meteorSpawner, context.Enemy.gameObject.transform.position, Quaternion.identity);
            var spawnerContext = new MeteorSpawnerContext
            {
                SpawnRadius = SpawnerRadius,
                SpawnAmount = SpawnAmount,
                SpawnCooldown = SpawnCooldown,
                MeteorPool = context.MeteorPool,
                Damage = damage,
                Radius = radius,
                Duration = chargeTime,
                TargetMask = targetMask
            };
            _spawner.Initialize(spawnerContext);
        }

        if (_spawner && _spawner._completed)
        {
            attackComplete = true;
            _spawner.DestroySpawner();
        }
    }
}
