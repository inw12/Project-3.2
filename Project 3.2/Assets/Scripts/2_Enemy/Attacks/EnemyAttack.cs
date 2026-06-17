using UnityEngine;

public struct EnemyAttackContext
{
    public Enemy Enemy;
    public EnemyAnimationController AnimationController;
    
    public ProjectilePool ProjectilePool;
    public ProjectilePool SecondaryProjectilePool;

    public Vector3 PlayerPosition;
    public LayerMask PlayerLayer;

    public Transform HitboxSpawn;
}

public abstract class EnemyAttack : ScriptableObject
{
    // Attack identifier
    public string attackName;
    public int attackID;

    // Attack runtime controllers
    [HideInInspector] public bool attackStarted;
    [HideInInspector] public bool attackComplete;

    [HideInInspector] public bool requiresMovement;

    public abstract void Initialize();
    public abstract void Attack(EnemyAttackContext context);
}

// ----------------------------------------------------------------------

#region * RANGED *
public abstract class EnemyRangedAttack : EnemyAttack
{
    [Header("Stats")]
    [SerializeField] protected float damage;
    [SerializeField] protected float fireRate;
    [SerializeField] protected float projectileSpeed;
    [SerializeField] protected float range;
}
#endregion

#region * AREA *
public abstract class EnemyAreaAttack       : EnemyAttack
{
    public enum AreaAttackShape
    {
        Circle,
        Box
    }

    [Header("Stats")]
    [SerializeField] protected float damage;
    protected AreaAttackShape _attackShape;
}
#endregion

public abstract class EnemyMeleeAttack      : EnemyAttack {}
public abstract class EnemyFocusAttack      : EnemyAttack {}
public abstract class EnemySpecialAttack    : EnemyAttack {}