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
    public abstract void Attack(EnemyAttackContext context);
}

public abstract class EnemyRangedAttack : EnemyAttack {}
public abstract class EnemyFocusAttack  : EnemyAttack {}
public abstract class EnemyMeleeAttack  : EnemyAttack {}
public abstract class EnemyZoneAttack   : EnemyAttack {}