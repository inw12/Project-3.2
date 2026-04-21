using UnityEngine;
using UnityEngine.Pool;
public class ProjectilePool : MonoBehaviour
{
    [SerializeField] private GameObject projectile;
    [Space]
    [SerializeField] private int defaultCapacity = 50;
    [SerializeField] private int maxCapacity = 100;

    private ObjectPool<GameObject> _pool;

    void Awake()
    {
        // Initialize Pool
        _pool = new ObjectPool<GameObject>
        (
            CreateProjectile,
            OnGetProjectile,
            OnReleaseProjectile,
            OnDestroyProjectile,
            true,
            defaultCapacity,
            maxCapacity
        );
    }

    private GameObject CreateProjectile()
    {
        var p = Instantiate(projectile, transform);
        return p;
    }

    private void OnGetProjectile(GameObject item)
    {
        item.SetActive(true);
    }

    private void OnReleaseProjectile(GameObject item)
    {
        item.SetActive(false);
    }

    private void OnDestroyProjectile(GameObject item) => Destroy(item);

    // (A) Simple Projectiles
    public void Get(ProjectileStats stats, Transform spawn)
    {
        GameObject item = _pool.Get();
        if (item.TryGetComponent(out Projectile p))
        {
            p.Initialize(this, stats, spawn);
        }
    }
    // (B) Projectiles that spawn other projectiles
    public void Get(ProjectileStats stats, Transform spawn, ProjectilePool secondaryPool)
    {
        GameObject item = _pool.Get();
        if (item.TryGetComponent(out Projectile p))
        {
            p.Initialize(this, secondaryPool, stats, spawn);
        }
    }
    // (C) Projectiles that track a target
    public void Get(ProjectileStats stats, Transform spawn, Transform target)
    {
        GameObject item = _pool.Get();
        if (item.TryGetComponent(out Projectile p))
        {
            p.Initialize(this, stats, spawn, target);
        }
    }
    // (D) Projectiles that track a target AND spawn other projectiles
    public void Get(ProjectileStats stats, Transform spawn, ProjectilePool secondaryPool, Transform target)
    {
        GameObject item = _pool.Get();
        if (item.TryGetComponent(out Projectile p))
        {
            p.Initialize(this, secondaryPool, stats, spawn, target);
        }
    }
    
    public void Release(GameObject item) => _pool.Release(item);
}
