using UnityEngine;

public class MeteorSpawnerContext
{
    // Spawner Stats
    public float SpawnRadius;
    public int SpawnAmount;
    public float SpawnCooldown;
    public MeteorPool MeteorPool;

    // Meteor Stats
    public float Damage;
    public float Radius;
    public float Duration;
    public LayerMask TargetMask;
}

public class MeteorSpawner : MonoBehaviour
{
    private MeteorSpawnerContext _context;

    private int _meteorCounter;
    private float _spawnTimer;

    [HideInInspector] public bool _completed;


    public void Initialize(MeteorSpawnerContext context)
    {
        _context = context;
        _meteorCounter = 0;
        _spawnTimer = context.SpawnCooldown;
    }

    void Update()
    {   
        if (_meteorCounter >= _context.SpawnAmount)
        {
            _completed = true;
        }

        _spawnTimer += Time.deltaTime;

        // Spawn new meteor
        if (_spawnTimer >= _context.SpawnCooldown && _meteorCounter < _context.SpawnAmount)
        {
            var stats = new MeteorStats
            {
                Damage      = _context.Damage,
                Radius      = _context.Radius,
                Duration    = _context.Duration,
                Spawn       = GetRandomSpawn(),
                TargetLayer = _context.TargetMask,
                ObjectPool  = _context.MeteorPool
            };
            _context.MeteorPool.Get(stats);

            _spawnTimer = 0f;
            _meteorCounter++;
        }
    }

    private Vector3 GetRandomSpawn()
    {
        var rand = Random.insideUnitSphere * _context.SpawnRadius;
        rand.y = 0f;
        return rand;
    }

    public void DestroySpawner() => Destroy(gameObject);
}
