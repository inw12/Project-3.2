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
    public float movementRange;

    [Header("Number of Projectiles per Attack")]
    public int projectilesPerShot;

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
            // * logic here *
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
            // Initialize Projectile Stats
            var stats = new ProjectileStats
            {
                Damage = damage,
                Speed = projectileSpeed,
                Range = range,
                Direction = Vector3.forward // *EDIT LATER*
            };

            // Get Projectile from Object Pool
            context.ProjectilePool.Get(stats, context.HitboxSpawn, context.PlayerLayer);

            // Reset fire rate timer
            _fireTimer = 0f;
        }
    }
    #endregion


    #region * Helper Functions
    // Returns a random position within a specified radius around the enemy
    private Vector3 GetRandomPosition()
    {
        var target = Random.insideUnitSphere * movementRange;
        target = Vector3.ProjectOnPlane(target, Vector3.up);
        return target;
    }
    #endregion
}
