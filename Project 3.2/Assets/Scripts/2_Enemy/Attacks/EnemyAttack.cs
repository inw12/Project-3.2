using UnityEngine;

public struct EnemyAttackContext
{
    public Enemy Enemy;
    public Transform HitboxSpawn;
    public ProjectilePool ProjectilePool;
    public ProjectilePool SecondaryProjectilePool;
    public LayerMask PlayerLayer;
}

public abstract class EnemyAttack : ScriptableObject
{
    // Attack identifier
    public int attackID;

    // Attack runtime controllers
    [HideInInspector] public bool attackStarted;
    [HideInInspector] public bool attackComplete;

    [HideInInspector] public bool requiresMovement;

    public abstract void Initialize();
    public abstract void Attack(EnemyAttackContext context);
}

public abstract class EnemyRangedAttack : EnemyAttack
{
    [Header("Stats")]
    [SerializeField] protected float damage;
    [SerializeField] protected float fireRate;
    [SerializeField] protected float projectileSpeed;
    [SerializeField] protected float range;
}
public abstract class EnemyFocusAttack  : EnemyAttack {}
public abstract class EnemyAreaAttack   : EnemyAttack {}
public abstract class EnemyMeleeAttack  : EnemyAttack {}