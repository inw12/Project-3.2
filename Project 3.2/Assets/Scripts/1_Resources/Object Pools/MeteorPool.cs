using UnityEngine;
using UnityEngine.Pool;
public class MeteorPool : MonoBehaviour
{
    [SerializeField] private GameObject meteorPrefab;
    [Space]
    [SerializeField] private int defaultCapacity = 50;
    [SerializeField] private int maxCapacity = 100;

    private ObjectPool<GameObject> _pool;

    void Awake()
    {
        // Initialize Pool
        _pool = new ObjectPool<GameObject>
        (
            CreateItem,
            OnGetItem,
            OnReleaseItem,
            OnDestroyItem,
            true,
            defaultCapacity,
            maxCapacity
        );
    }

    private GameObject CreateItem()
    {
        var p = Instantiate(meteorPrefab, null);
        return p;
    }

    private void OnGetItem(GameObject item)
    {
        item.SetActive(true);
    }

    private void OnReleaseItem(GameObject item)
    {
        item.SetActive(false);
    }

    private void OnDestroyItem(GameObject item) => Destroy(item);

    public void Get(MeteorStats stats)
    {
        GameObject item = _pool.Get();
        if (item.TryGetComponent(out Meteor m))
            m.Initialize(stats);
    }
    
    public void Release(GameObject item) 
    {
        _pool.Release(item);
    }
}
