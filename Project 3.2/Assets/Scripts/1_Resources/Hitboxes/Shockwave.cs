using System.Collections.Generic;
using UnityEngine;
public struct ShockwaveStats
{
    // the "look"
    public float Radius;    // how big?
    public float Width;     // how wide? (the width of the 'ring')
    public float Duration;

    // the functionality
    public float Damage;
    public LayerMask TargetLayer;
    public Vector3 Spawn;
    public ShockwavePool ObjectPool;
}
public class Shockwave : MonoBehaviour
{
    #region * Variables --------------------------------------------------
    private ShockwaveStats _stats;

    // Mesh Variables
    [SerializeField] private Material ringMaterial;
    [SerializeField] private int meshSegments;
    private MeshFilter   _meshFilter;
    private MeshRenderer _meshRenderer;
    private MaterialPropertyBlock _propBlock;
    private static readonly int OpacityID = Shader.PropertyToID("_Opacity");

    // Hitbox Variables
    private readonly HashSet<Collider> _hits = new();   // recording what has/hasn't been hit

    // Progress tracking
    private float _timeElapsed;
    #endregion


    public void Initialize(ShockwaveStats stats)
    {
        ResetShockwave();
        _stats = stats;
        transform.position = _stats.Spawn;

        // Mesh
        _meshFilter     = gameObject.TryGetComponent(out MeshFilter mf)     ? mf : gameObject.AddComponent<MeshFilter>();
        _meshRenderer   = gameObject.TryGetComponent(out MeshRenderer mr)   ? mr : gameObject.AddComponent<MeshRenderer>();

        _meshRenderer.material = ringMaterial;
        _propBlock             = new MaterialPropertyBlock();
        _meshFilter.mesh = new Mesh { name = "ShockwaveMesh" };
    }


    #region * Hit Detection
    void Update()
    {
        var deltaTime = Time.deltaTime;
        _timeElapsed += deltaTime;

        // Return to object pool after duation
        if (_timeElapsed >= _stats.Duration) _stats.ObjectPool.Release(gameObject);


        var t           = Mathf.Clamp01(_timeElapsed / _stats.Duration);
        var outerRadius = Mathf.Lerp(0f, _stats.Radius, t);
        var innerRadius = Mathf.Max(0f, outerRadius - _stats.Width);

        // All colliders within the outer radius
        Collider[] outerHits = Physics.OverlapSphere
        (
            transform.position, 
            outerRadius, 
            _stats.TargetLayer
        );

        foreach (Collider hit in outerHits)
        {
            if (_hits.Contains(hit)) continue;

            // Check the collider is outside the inner radius — i.e. in the ring band
            var dist = Vector3.Distance(transform.position, hit.ClosestPoint(transform.position));
            if (dist >= innerRadius)
            {
                if (hit.gameObject.TryGetComponent(out IDamageable i))
                {
                    _hits.Add(hit);
                    i.DecreaseHealth(_stats.Damage);
                }
            }
        }
    }
    #endregion


    #region * Movement
    void FixedUpdate()
    {
        float t           = Mathf.Clamp01(_timeElapsed / _stats.Duration);
        float outerRadius = Mathf.Lerp(0f, _stats.Radius, t);

        // Inner radius is always exactly _ringWidth behind the outer edge
        float innerRadius = Mathf.Max(0f, outerRadius - _stats.Width);

        float opacity = 1f - Mathf.Pow(t, 2f);

        RebuildMesh(innerRadius, outerRadius);

        _meshRenderer.GetPropertyBlock(_propBlock);
        _propBlock.SetFloat(OpacityID, opacity);
        _meshRenderer.SetPropertyBlock(_propBlock);
    }
    #endregion


    #region * Helper Functions
    private void RebuildMesh(float innerRadius, float outerRadius)
    {
        Mesh mesh      = _meshFilter.mesh;
        int  vertCount = meshSegments * 2;

        Vector3[] vertices  = new Vector3[vertCount];
        Vector2[] uvs       = new Vector2[vertCount];
        int[]     triangles = new int[meshSegments * 6];

        for (int i = 0; i < meshSegments; i++)
        {
            float angle = (float)i / meshSegments * Mathf.PI * 2f;
            float cos   = Mathf.Cos(angle);
            float sin   = Mathf.Sin(angle);

            // Inner and outer vertices use the actual world-space radii directly
            vertices[i * 2]     = new Vector3(cos * innerRadius, 0f, sin * innerRadius);
            vertices[i * 2 + 1] = new Vector3(cos * outerRadius, 0f, sin * outerRadius);

            float u = (float)i / meshSegments;
            uvs[i * 2]     = new Vector2(u, 0f);
            uvs[i * 2 + 1] = new Vector2(u, 1f);

            int ti     = i * 6;
            int vi     = i * 2;
            int nextVi = (vi + 2) % vertCount;

            triangles[ti]     = vi;
            triangles[ti + 1] = vi + 1;
            triangles[ti + 2] = nextVi;
            triangles[ti + 3] = nextVi;
            triangles[ti + 4] = vi + 1;
            triangles[ti + 5] = nextVi + 1;
        }

        mesh.Clear();
        mesh.vertices  = vertices;
        mesh.triangles = triangles;
        mesh.uv        = uvs;
        mesh.RecalculateNormals();
    }
    public void ResetShockwave()
    {
        _timeElapsed = 0f;
        _hits.Clear();
    }
    #endregion
}