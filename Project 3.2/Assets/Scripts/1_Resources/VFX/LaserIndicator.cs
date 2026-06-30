using UnityEngine;

public class LaserIndicator : MonoBehaviour
{
    [Header("Attack Indicator")]
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Transform start;
    [SerializeField] private Transform end;
    [Space]
    [SerializeField] private float blinkDuration;
    private float _blinkTimer;
    private bool _isBlinking;

    [Header("VFX | Charge Up")]
    [SerializeField] private ParticleSystem auraParticles;
    [SerializeField] private ParticleSystem energyParticles;
    private ParticleSystem.MainModule auraMain;
    private ParticleSystem.MainModule energyMain;

    [Header("VFX | Fire")]
    [SerializeField] private ParticleSystem shootParticles;

    private MeshRenderer _startMesh;
    private MeshRenderer _endMesh;
    private MaterialPropertyBlock _mpb;
    private static readonly int BlinkProgress = Shader.PropertyToID("_BlinkProgress");

    private float _duration;
    private float _timer;

    public void Initialize(Vector3 startPos, Vector3 endPos, float duration, Vector3 vfxSpawn)
    {
        start.position = startPos;
        end.position = endPos;

        lineRenderer.SetPosition(0, start.position);
        lineRenderer.SetPosition(1, end.position);

        _blinkTimer = _timer = 0f;
        _isBlinking = true;

        _startMesh = start.GetComponent<MeshRenderer>();
        _endMesh = end.GetComponent<MeshRenderer>();
        _mpb = new MaterialPropertyBlock();

        _duration = duration;

        // Particle System Stuff
        auraMain    = auraParticles.main;
        energyMain  = energyParticles.main;
        auraMain.startLifetime = energyMain.duration = _duration;

        vfxSpawn.y = 0.25f;
        auraParticles.transform.position = vfxSpawn;
        auraParticles.Play();
    }

    void Update()
    {
        _timer += Time.deltaTime;

        lineRenderer.SetPosition(0, start.position);
        lineRenderer.SetPosition(1, end.position);

        // "Blinking" Logic
        _blinkTimer = _isBlinking ? _blinkTimer += Time.deltaTime : _blinkTimer -= Time.deltaTime;
        var p = Mathf.Clamp01(_blinkTimer / blinkDuration);
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

        TryToDestroy();
    }

    public void SetStartPosition(Vector3 position)  => start.position = position;
    public void SetEndPosition(Vector3 position)    => end.position = position;
    public void PlayShootEffects(Vector3 origin) 
    {
        shootParticles.transform.position = origin;
        shootParticles.Play();
    }

    private void TryToDestroy()
    {
        if (_timer >= _duration)
        {
            lineRenderer.enabled = false;
            start.gameObject.SetActive(false);
            end.gameObject.SetActive(false);

            if (!auraParticles.isPlaying || !energyParticles.isPlaying || !shootParticles.isPlaying)
            {
                Destroy(gameObject);
            }
        }
    }
}
