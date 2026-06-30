using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LineDrop : MonoBehaviour
{
    [SerializeField] private ParticleSystem particles;
    [SerializeField] private Transform spawn;
    [SerializeField] private float speed;
    [SerializeField] private float startWidth;

    private LineRenderer _lr;

    private float _distanceThisFrame;

    private Vector3 _current;
    private Vector3 _next;

    private bool _floorHit;

    private bool _particlesTriggered;

    void Start()
    {
        _lr = GetComponent<LineRenderer>();
        _lr.SetPosition(0, spawn.position);
        _lr.SetPosition(1, spawn.position);
        _lr.startWidth = startWidth;

        _current    = _lr.GetPosition(0);
        _next       = _lr.GetPosition(1);

        _distanceThisFrame = 0f;

        _floorHit = _particlesTriggered = false;
    }

    void Update()
    {
        if (_lr.GetPosition(0).y <= 0f && !particles.isPlaying)
        {
            Destroy(gameObject);
        }

        if (!_floorHit)
        {
            _distanceThisFrame = speed * Time.deltaTime;
            _current = _lr.GetPosition(1);
            _next = _current + (-Vector3.up * _distanceThisFrame);

            _lr.SetPosition(1, _next);
        }
        else
        {
            _distanceThisFrame = speed * Time.deltaTime;
            _current = _lr.GetPosition(0);
            _next = _current + (-Vector3.up * _distanceThisFrame);

            _lr.SetPosition(0, _next);
        }

        if (_lr.GetPosition(1).y <= 0f)
        {
            _floorHit = true;

            if (!_particlesTriggered)
            {
                _particlesTriggered = true;
                particles.Play();
            }
        }
    }
}
