using System.Collections.Generic;
using UnityEngine;

public struct LaserStats
{
    public float Damage;
    public float Speed;
    public float Range;
    public float Width;
    public Vector3 Origin;
    public Vector3 Direction;
    public LayerMask TargetLayer;
}

[RequireComponent(typeof(LineRenderer))]
public class Laser : MonoBehaviour
{
    // * delete later *
    // Temp Stats
    public float damage;
    public float speed;
    public float range;
    public float width;
    public LayerMask targetLayer;
    // ****************

    [SerializeField] private float hitboxRadius;
    [SerializeField] private float dissipateSpeed;

    private LineRenderer _lineRenderer;
    private LaserStats _stats;
    
    protected readonly RaycastHit[] _hits = new RaycastHit[5];  // buffer array for 'SphereCastAllNonAlloc'
    private readonly HashSet<Collider> _alreadyHit = new();     // to prevent multiple detections on the same object

    private float _distanceThisFrame;
    private float _distanceTraveled;

    private bool _isActive;

    private Vector3 _current;
    private Vector3 _next;

    void Start()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.SetPosition(0, transform.position);
        _lineRenderer.SetPosition(1, transform.position);
        _lineRenderer.startWidth = width;

        _current    = _lineRenderer.GetPosition(0);
        _next       = _lineRenderer.GetPosition(1);

        _distanceThisFrame = _distanceTraveled = 0f;

        _isActive = true;

        _alreadyHit.Clear();
    }

    public void Initialize(LaserStats stats)
    {
        _stats = stats;

        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.SetPosition(0, _stats.Origin);
        _lineRenderer.SetPosition(1, _stats.Origin);
        _lineRenderer.startWidth = _stats.Width;

        _current    = _lineRenderer.GetPosition(0);
        _next       = _lineRenderer.GetPosition(1);

        _distanceThisFrame = _distanceTraveled = 0f;

        _isActive = true;

        _alreadyHit.Clear();
    }


    void Update()
    {
        _distanceThisFrame = speed * Time.deltaTime;
        _current = _lineRenderer.GetPosition(1);
        _next = _current + (Vector3.forward * _distanceThisFrame);

        // 1. Perform Movement
        if (_isActive)
        {
            _lineRenderer.SetPosition(1, _next);
            _distanceTraveled += _distanceThisFrame;
        }

        // 2. Check for collisions
        ScanForHits();

        // 3. Check if de-spawn conditions are met
        TryDespawn();
    }


    #region * Helper Functions --------------------------------------------------
    // Hitbox Scanning
    private void ScanForHits()
    {
        var direction = (_next - _current).normalized;
        var hits = Physics.SphereCastNonAlloc
        (
            _current,
            hitboxRadius,
            direction,
            _hits,
            _distanceThisFrame,
            targetLayer
        );

        if (hits > 0) HandleHit();
    }

    // Hit Effect
    private void HandleHit()
    {
        _isActive = false;

        var hit = _hits[0].collider;
        if (_alreadyHit.Add(hit))
        {
            if (hit.gameObject.TryGetComponent(out IDamageable i))
            {
                i.DecreaseHealth(damage);
            }
        }
    }

    // Lerp to 0 -> Destroy()
    private void Despawn()
    {
        if (_isActive) _isActive = false;

        // Reduce laser width over time
        _lineRenderer.startWidth = Mathf.Lerp
        (
            _lineRenderer.startWidth,
            0f,
            1f - Mathf.Exp(-dissipateSpeed * Time.deltaTime)
        );

        // Remove object once width reaches a certain point
        if (_lineRenderer.startWidth <= 0.05f) 
        {
            Destroy(gameObject);
        }
    }

    private void TryDespawn()
    {
        if (_distanceThisFrame > range || !_isActive)
        {
            // Reduce laser width over time
            _lineRenderer.startWidth = Mathf.Lerp
            (
                _lineRenderer.startWidth,
                0f,
                1f - Mathf.Exp(-dissipateSpeed * Time.deltaTime)
            );

            // Remove object once width reaches a certain point
            if (_lineRenderer.startWidth <= 0.05f) 
            {
                Destroy(gameObject);
            }
        }
    }
    #endregion
}
