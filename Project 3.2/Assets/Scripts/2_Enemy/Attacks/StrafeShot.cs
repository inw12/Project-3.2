using UnityEngine;
[CreateAssetMenu(menuName = "Enemy Attacks/Ranged/StrafeShot")]
public class StrafeShot : EnemyRangedAttack
{
    [Header("Stats")]
    public float damage;
    public float fireRate;
    public float projectileSpeed;
    public float range;

    [Header("Movement")]
    public float minMoveDistance;
    public float maxMoveDistance;

    private float _fireTimer;           // Fire rate control

    #region * Initialization
    public override void Initialize()
    {
        requiresMovement = true;

        _fireTimer = fireRate;

        attackStarted = false;
        attackComplete = false;
    }
    #endregion


    #region * Attack Implementation
    public override void Attack(EnemyAttackContext context)
    {
        // Attack START
        if (!attackStarted)
        {
            attackStarted = true;

            // 1. Find position to move towards
            var position = GetRandomPosition();

            // 2. Apply movement
            context.Enemy.SetMovementTarget(position);

            // 3. Turn on arm animation rig
            context.Enemy.ArmRigEnabled(true);
        }

        // Attack END
        var currentState = context.Enemy.GetState();
        attackComplete = currentState.CurrentAction is not EnemyAction.Attack;
        // ^    this script isn't called unless the current state is "Attack"
        //      so this check shooould work...


        // Update timers
        var deltaTime = Time.deltaTime;
        _fireTimer += deltaTime;


        // * Attack IDLE
        //      - Fires projectiles in an arc (shotgun-blast)
        if (_fireTimer >= fireRate && !attackComplete)
        {
            // Get firing direction
            var start = Vector3.ProjectOnPlane(context.Enemy.transform.position, Vector3.up);
            var end = Vector3.ProjectOnPlane(context.Enemy.GetPlayerPosition(), Vector3.up);
            var direction = (end - start).normalized;

            // Initialize Projectile Stats
            var stats = new ProjectileStats
            {
                Damage = damage,
                Speed = projectileSpeed,
                Range = range,
                Direction = direction
            };

            // *testing*
            // adjust projectile spawn height
            var spawnPos = context.HitboxSpawn;
            var spawnActual = new Vector3(spawnPos.position.x, 1f, spawnPos.position.z);
            spawnPos.position = spawnActual;

            // Get Projectile from Object Pool
            context.ProjectilePool.Get(stats, spawnPos, context.PlayerLayer);

            // Reset fire rate timer
            _fireTimer = 0f;
        }
    }
    #endregion


    #region * Helper Functions
    // Returns a random position within a specified radius around the enemy
    private Vector3 GetRandomPosition()
    {
        var range = Random.Range(minMoveDistance, maxMoveDistance);
        var target = Random.insideUnitSphere * range;
        target = Vector3.ProjectOnPlane(target, Vector3.up);
        return target;
    }
    #endregion
}
