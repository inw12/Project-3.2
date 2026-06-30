using UnityEngine;

public class LaserIndicator : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Transform start;
    [SerializeField] private Transform end;
    [Space]
    [SerializeField] private Color baseColor;
    [SerializeField] private Color blinkColor;
    [Space]
    [SerializeField] private float blinkDuration;

    private MeshRenderer _startMesh;
    private MeshRenderer _endMesh;
    private MaterialPropertyBlock _mpb;

    private float _timer;
    private bool _isBlinking;

    private static readonly int BlinkProgress = Shader.PropertyToID("_BlinkProgress");

    //void Start()
    //{
    //    lineRenderer.SetPosition(0, start.position);
    //    lineRenderer.SetPosition(1, end.position);
    //    _timer = 0f;
    //    _isBlinking = true;
    //    _startMesh = start.GetComponent<MeshRenderer>();
    //    _endMesh = end.GetComponent<MeshRenderer>();
    //    _mpb = new MaterialPropertyBlock();
    //}

    public void Initialize(Vector3 startPos, Vector3 endPos)
    {
        start.position = startPos;
        end.position = endPos;

        lineRenderer.SetPosition(0, start.position);
        lineRenderer.SetPosition(1, end.position);

        _timer = 0f;
        _isBlinking = true;

        _startMesh = start.GetComponent<MeshRenderer>();
        _endMesh = end.GetComponent<MeshRenderer>();
        _mpb = new MaterialPropertyBlock();
    }

    void Update()
    {
        lineRenderer.SetPosition(0, start.position);
        lineRenderer.SetPosition(1, end.position);

        // "Blinking" Logic
        _timer = _isBlinking ? _timer += Time.deltaTime : _timer -= Time.deltaTime;
        var p = Mathf.Clamp01(_timer / blinkDuration);
        if (p >= 1f) _isBlinking = false;
        if (p <= 0f) _isBlinking = true;   

        // Update Shader Graph
        _endMesh.GetPropertyBlock(_mpb);
        _mpb.SetFloat(BlinkProgress, p);
        _endMesh.SetPropertyBlock(_mpb);

        _mpb = new MaterialPropertyBlock();

        _startMesh.GetPropertyBlock(_mpb);
        _mpb.SetFloat(BlinkProgress, p);
        _startMesh.SetPropertyBlock(_mpb);
    }

    public void SetStartPosition(Vector3 position)  => start.position = position;
    public void SetEndPosition(Vector3 position)    => end.position = position;
}
