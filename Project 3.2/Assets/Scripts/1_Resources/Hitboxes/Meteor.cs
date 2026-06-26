using UnityEngine;

public struct MeteorStats
{
    public float Damage;
    public float Radius;
    public Vector3 Spawn;
    public LayerMask TargetLayer;
    public MeteorPool ObjectPool;
}

public class Meteor : MonoBehaviour
{
    // * delete later *
    public float damage;
    public float radius;
    public LayerMask targetMask;
    public AttackIndicator attackIndicator;
    public float duration;

    private MeteorStats _stats;

    // hit detection
    private Collider[] _hits = new Collider[5];

    public void Initialize(MeteorStats stats)
    {
        _stats = stats;
    }

    void OnEnable()     => attackIndicator.OnComplete += HandleHit;
    void OnDisable()    => attackIndicator.OnComplete -= HandleHit;

    void Start()
    {
        attackIndicator.Initialize(duration);
        transform.localScale = new Vector3(radius, radius, radius);
    }

    void Update()
    {
        
    }

    private void HandleHit()
    {
        // Scan for hits
        var hits = Physics.OverlapSphereNonAlloc
        (
            transform.position,
            radius,
            _hits,
            targetMask
        );

        // Hit detection
        if (hits > 0)
        {
            var hit = _hits[0];

            if (hit.gameObject.TryGetComponent(out IDamageable i))
            {
                i.DecreaseHealth(damage);
                Destroy(gameObject);
            }
        }
    }
}
