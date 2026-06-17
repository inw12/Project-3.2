using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShockwaveHitbox : MonoBehaviour
{
    [SerializeField] private float  _maxRadius    = 8f;
    [SerializeField] private float  _ringWidth    = 0.6f; // Should loosely match visual thickness
    [SerializeField] private LayerMask _hitLayers;

    // Tracks already-hit objects so they can't be hit twice by the same shockwave
    private readonly HashSet<Collider> _hitRegistry = new();

    public event System.Action<Collider> OnHit;


    public void Expand(float duration)
    {
        StartCoroutine(ExpandRoutine(duration));
    }
    public IEnumerator ExpandRoutine(float duration)
    {
        float elapsed     = 0f;
        float prevRadius  = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t           = Mathf.Clamp01(elapsed / duration);
            float outerRadius = Mathf.Lerp(0f, _maxRadius, t);
            float innerRadius = Mathf.Max(0f, outerRadius - _ringWidth);

            // All colliders within the outer radius
            Collider[] outerHits = Physics.OverlapSphere(transform.position, outerRadius, _hitLayers);

            foreach (Collider hit in outerHits)
            {
                if (_hitRegistry.Contains(hit)) continue;

                // Check the collider is outside the inner radius — i.e. in the ring band
                float dist = Vector3.Distance(transform.position, hit.ClosestPoint(transform.position));
                if (dist >= innerRadius)
                {
                    _hitRegistry.Add(hit);
                    OnHit?.Invoke(hit);
                }
            }

            prevRadius = outerRadius;
            yield return null;
        }
    }
}