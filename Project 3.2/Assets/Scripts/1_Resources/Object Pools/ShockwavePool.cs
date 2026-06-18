using UnityEngine;
using UnityEngine.Pool;
public class ShockwavePool : MonoBehaviour
{
    [SerializeField] private GameObject shockwavePrefab;
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
        var p = Instantiate(shockwavePrefab, null);
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

    public void Get(ShockwaveStats stats)
    {
        GameObject item = _pool.Get();
        if (item.TryGetComponent(out Shockwave s))
        {
            s.Initialize(stats);
        }
    }
    
    public void Release(GameObject item) => _pool.Release(item);
}
