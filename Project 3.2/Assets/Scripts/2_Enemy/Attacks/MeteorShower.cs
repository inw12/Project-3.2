using UnityEngine;
[CreateAssetMenu(menuName = "Enemy Attacks/Area/MeteorShower")]
public class MeteorShower : EnemyAreaAttack
{
    [Space]
    [SerializeField] private MeteorSpawner meteorSpawner;
    [SerializeField] private float timeToIdle;      // amount of time that 'Attack' is called for before returning to idle
    private float _idleTimer;

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
        _idleTimer = 0f;
    }

    public override void Attack(EnemyAttackContext context)
    {
        _idleTimer += Time.deltaTime;

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

        if (_idleTimer >= timeToIdle)
        {
            attackComplete = true;
            _spawner.DestroySpawner();
        }
    }
}
