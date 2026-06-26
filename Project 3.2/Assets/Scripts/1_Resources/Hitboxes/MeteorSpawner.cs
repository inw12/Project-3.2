using UnityEngine;
public class MeteorSpawner : MonoBehaviour
{
    [Header("Spawner Stats")]
    [SerializeField] private float spawnRadius;
    [SerializeField] private int spawnAmount;
    [SerializeField] private float cooldown;
    [SerializeField] private MeteorPool meteorPool;
    [Header("Meteor Stats")]
    [SerializeField] private float damage;
    [SerializeField] private float radius;
    [SerializeField] private float duration;
    [SerializeField] private LayerMask targetMask;

    private int _meteorCounter;
    private float _spawnTimer;

    void Start()
    {
        _meteorCounter = 0;
        _spawnTimer = 0f;
    }

    void Update()
    {   
        if (_meteorCounter >= spawnAmount)
        {
            Destroy(gameObject);
        }

        _spawnTimer += Time.deltaTime;
        if (_spawnTimer >= cooldown)
        {
            var stats = new MeteorStats
            {
                Damage      = damage,
                Radius      = radius,
                Duration    = duration,
                Spawn       = GetRandomSpawn(),
                TargetLayer = targetMask,
                ObjectPool  = meteorPool
            };
            meteorPool.Get(stats);

            _spawnTimer = 0f;
            _meteorCounter++;
        }
    }

    private Vector3 GetRandomSpawn()
    {
        var rand = Random.insideUnitSphere * spawnRadius;
        rand.y = 0f;
        return rand;
    }
}
