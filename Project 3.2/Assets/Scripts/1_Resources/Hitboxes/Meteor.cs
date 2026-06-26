using UnityEngine;

public struct MeteorStats
{
    public float Damage;
    public float Radius;
    public float Duration;

    public Vector3 Spawn;
    public LayerMask TargetLayer;
    public MeteorPool ObjectPool;
}

public class Meteor : MonoBehaviour
{
    [SerializeField] private AttackIndicator attackIndicator;
    private MeteorStats _stats;

    // hit detection
    private readonly Collider[] _hits = new Collider[5];

    public void Initialize(MeteorStats stats)
    {
        _stats = stats;

        attackIndicator.Initialize(_stats.Duration);
        transform.position = _stats.Spawn;
        transform.localScale = new Vector3(_stats.Radius, _stats.Radius, _stats.Radius);
    }

    void Update()
    {
        attackIndicator.UpdateIndicator();
    }

    private void HandleHit()
    {
        // Scan for hits
        var hits = Physics.OverlapSphereNonAlloc
        (
            transform.position,
            _stats.Radius,
            _hits,
            _stats.TargetLayer
        );

        // Hit detection
        if (hits > 0)
        {
            var hit = _hits[0];

            if (hit.gameObject.TryGetComponent(out IDamageable i))
            {
                i.DecreaseHealth(_stats.Damage);
            }
        }
        
        Destroy(gameObject);
    }

    void OnEnable()     => attackIndicator.OnComplete += HandleHit;
    void OnDisable()    => attackIndicator.OnComplete -= HandleHit;
}
