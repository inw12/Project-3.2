using UnityEngine;

public struct EnemyAttackContext
{
    public Enemy Enemy;
    public Transform HitboxSpawn;
    public ProjectilePool ProjectilePool;
    public LayerMask PlayerLayer;
}

public abstract class EnemyAttack : ScriptableObject
{
    // Attack identifier
    public int attackID;

    // Attack runtime controllers
    protected bool _attackStarted;
    protected bool _attackComplete;
    [HideInInspector] public bool attackComplete;

    [HideInInspector] public bool requiresMovement;

    public abstract void Initialize();
    public abstract void Attack(EnemyAttackContext context);
}

public abstract class EnemyRangedAttack : EnemyAttack {}
public abstract class EnemyFocusAttack  : EnemyAttack {}
public abstract class EnemyAreaAttack   : EnemyAttack {}
public abstract class EnemyMeleeAttack  : EnemyAttack {}