using UnityEngine;
public class Projectile_Basic : Projectile
{
    // Travels in a straight line towards a target position
    protected override void Move()
    {
        // Update distance to travel this frame
        _distanceThisFrame = _stats.Speed * Time.deltaTime;

        // Travel forward
        transform.position += _stats.Direction * _distanceThisFrame;
        
        // Return to object pool after travelling a certain distance;
        _distanceTraveled += _distanceThisFrame;
        if (_distanceTraveled >= _stats.Range)
        {
            _pool.Release(gameObject);
        }
    }

    // Handle projectile hit
    public override void OnHit(Collider other)
    {
        if (other.gameObject.TryGetComponent(out IDamageable e))
        {
            e.DecreaseHealth(_stats.Damage);
            _pool.Release(gameObject);
        }
    }
}
