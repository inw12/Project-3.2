using UnityEngine;
[CreateAssetMenu(menuName = "Enemy Attacks/Area/MeteorShower")]
public class MeteorShower : EnemyAreaAttack
{
    [Header("Attack Indicator")]
    [SerializeField] private float radius;      // size of attack indicator
    [SerializeField] private float chargeTime;  // how long until indicator is filled?

    public override void Initialize()
    {
        _attackShape = AreaAttackShape.Circle;
        requiresMovement = false;

        attackStarted = attackComplete = false;
    }

    /// * DESIRED BEHAVIOR:
    ///     1. Finds n random positions to attack
    ///     2. Starts a coroutine to start the attack
    ///     3. Spawns n "meteors" (attack indicators -> active hitbox) over the course of m seconds
    /// 
    /// - Once triggered, runs INDEPENDENTLY from 'EnemyAI'
    /// - "Enemy snaps his fingers, and multiple meteors spawn around him (cascading; not all at once)."
    /// - "Enemy should be free to act upon triggering this action." 
    public override void Attack(EnemyAttackContext context)
    {
        // * implementation here *
    }
}
