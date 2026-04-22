using UnityEngine;
public struct ProjectileStats
{
    public float Damage;        
    public float Speed;         
    public float Range;
    public Vector3 Direction; 
}
public abstract class Projectile : MonoBehaviour
{
    [SerializeField] protected float hitboxRadius;

    protected ProjectileStats _stats;
    protected LayerMask _targetLayer;

    protected ProjectilePool _pool;
    protected ProjectilePool _poolSecondary;
    protected Transform _target;

    protected readonly Collider[] _hits = new Collider[10];

    // Range Management
    protected float _distanceThisFrame;
    protected float _distanceTraveled;

    #region *--- Initialization Methods --------------------------------------------------*
    // (A) Simple Projectiles
    public virtual void Initialize(ProjectilePool pool, ProjectileStats stats, Transform spawn, LayerMask targetLayer)
    {
        // Object Pool
        _pool = pool;

        // Projectile Stats
        _stats = stats;

        // Spawn Position
        transform.position = spawn.position;

        _distanceTraveled = 0f;
    }
    // (B) Projectiles that spawn other projectiles
    public virtual void Initialize(ProjectilePool pool, ProjectilePool poolSecondary, ProjectileStats stats, Transform spawn, LayerMask targetLayer)
    {
        // Object Pools
        _pool = pool;
        _poolSecondary = poolSecondary;

        // Projectile Stats
        _stats = stats;

        // Spawn Position
        transform.position = spawn.position;

        _distanceTraveled = 0f;
    }
    // (C) Projectiles that track a target
    public virtual void Initialize(ProjectilePool pool, ProjectileStats stats, Transform spawn, LayerMask targetLayer, Transform target)
    {
        // Object Pools
        _pool = pool;

        // Tracking Target
        _target = target;

        // Projectile Stats
        _stats = stats;

        // Spawn Position
        transform.position = spawn.position;

        _distanceTraveled = 0f;
    }
    // (D) Projectiles that track a target AND spawn other projectiles
    public virtual void Initialize(ProjectilePool pool, ProjectilePool poolSecondary, ProjectileStats stats, Transform spawn, Transform target, LayerMask targetLayer)
    {
        // Object Pools
        _pool = pool;
        _poolSecondary = poolSecondary;

        // Tracking Target
        _target = target;

        // Projectile Stats
        _stats = stats;

        // Spawn Position
        transform.position = spawn.position;

        _distanceTraveled = 0f;
    }
    #endregion

    // Collision Detection
    protected virtual void Update()
    {
        // OverlapSphere to detect collisions in 'HittableLayers'
        var hits = Physics.OverlapSphereNonAlloc
        (
            transform.position,
            hitboxRadius,
            _hits,
            _targetLayer
        );

        // Handle hit if detected
        if (hits > 0) OnHit(_hits[0]);
    }

    public virtual void OnHit(Collider other)
    {
        if (other.gameObject.TryGetComponent(out IDamageable i))
        {
            i.DecreaseHealth(_stats.Damage);
            _pool.Release(gameObject);
        }
    }

    // Projectile Movement
    protected virtual void FixedUpdate() => Move();
    protected abstract void Move();
}
