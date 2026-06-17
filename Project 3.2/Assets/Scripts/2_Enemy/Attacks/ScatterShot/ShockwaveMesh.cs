using System.Collections;
using UnityEngine;

public class ShockwaveMesh : MonoBehaviour
{
    [Header("Ring Settings")]
    [SerializeField] private float _maxRadius     = 8f;
    [SerializeField] private float _ringWidth     = 0.3f;  // Fixed world-space width — never changes
    [SerializeField] private int   _ringSegments  = 64;

    [Header("Visual")]
    [SerializeField] private Material _ringMaterial;

    private MeshFilter   _meshFilter;
    private MeshRenderer _meshRenderer;
    private MaterialPropertyBlock _propBlock;

    private static readonly int OpacityID = Shader.PropertyToID("_Opacity");

    private void Awake()
    {
        _meshFilter            = gameObject.AddComponent<MeshFilter>();
        _meshRenderer          = gameObject.AddComponent<MeshRenderer>();
        _meshRenderer.material = _ringMaterial;
        _propBlock             = new MaterialPropertyBlock();

        // Assign an empty mesh — it gets built every frame now
        _meshFilter.mesh = new Mesh { name = "ShockwaveRing" };
    }

    public void Expand(float duration) => StartCoroutine(ExpandRoutine(duration));
    public IEnumerator ExpandRoutine(float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t           = Mathf.Clamp01(elapsed / duration);
            float outerRadius = Mathf.Lerp(0f, _maxRadius, t);

            // Inner radius is always exactly _ringWidth behind the outer edge
            float innerRadius = Mathf.Max(0f, outerRadius - _ringWidth);

            float opacity = 1f - Mathf.Pow(t, 2f);

            RebuildMesh(innerRadius, outerRadius);

            _meshRenderer.GetPropertyBlock(_propBlock);
            _propBlock.SetFloat(OpacityID, opacity);
            _meshRenderer.SetPropertyBlock(_propBlock);

            yield return null;
        }

        Destroy(gameObject);
    }

    private void RebuildMesh(float innerRadius, float outerRadius)
    {
        Mesh mesh      = _meshFilter.mesh;
        int  vertCount = _ringSegments * 2;

        Vector3[] vertices  = new Vector3[vertCount];
        Vector2[] uvs       = new Vector2[vertCount];
        int[]     triangles = new int[_ringSegments * 6];

        for (int i = 0; i < _ringSegments; i++)
        {
            float angle = (float)i / _ringSegments * Mathf.PI * 2f;
            float cos   = Mathf.Cos(angle);
            float sin   = Mathf.Sin(angle);

            // Inner and outer vertices use the actual world-space radii directly
            vertices[i * 2]     = new Vector3(cos * innerRadius, 0f, sin * innerRadius);
            vertices[i * 2 + 1] = new Vector3(cos * outerRadius, 0f, sin * outerRadius);

            float u = (float)i / _ringSegments;
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
}