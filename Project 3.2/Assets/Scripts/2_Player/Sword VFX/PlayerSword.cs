using System.Collections;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PlayerSword : MonoBehaviour
{
    // Trail Renderer
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private PlayerAnimationController animationController;

    // Line Renderer
    private LineRenderer _lr;
    private Vector2 _dimensions;
    private float _currentLength;

    #region * Initialization
    public void Initialize(Vector2 dimensions, float trailSize, float trailDuration)
    {
        // Line Renderer
        _lr = GetComponent<LineRenderer>();
        _lr.enabled = false;

        _dimensions = dimensions;
        _lr.positionCount = 2;
        _lr.startWidth = _lr.endWidth = _dimensions.x;


        // Trail Renderer
        trailRenderer.enabled = true;
        trailRenderer.emitting = false;
        // - position
        var targetPos = new Vector3(0f, 0f, dimensions.y / 2f);
        trailRenderer.gameObject.transform.localPosition = targetPos;
        // - starting width
        trailRenderer.startWidth = dimensions.y * trailSize;
        // - time/duration
        trailRenderer.time = trailDuration;
    }
    void OnEnable()
    {
        animationController.OnSwordTrailEnabled += EnableTrailRenderer;
        animationController.OnSwordTrailDisabled += DisableTrailRenderer;
    }
    void OnDisable()
    {
        animationController.OnSwordTrailEnabled -= EnableTrailRenderer;
        animationController.OnSwordTrailDisabled += DisableTrailRenderer;
    }
    #endregion

    void LateUpdate()
    {
        if (_lr.enabled)
            UpdateLine();
    }

    private void UpdateLine()
    {
        var origin = transform.position;
        var tip = origin + transform.forward * (_dimensions.y * _currentLength);

        _lr.SetPosition(0, origin);
        _lr.SetPosition(1, tip);
    }

    #region * On/Off Coroutines
    public IEnumerator Open(float duration)
    {
        _lr.enabled = true;
        var timer = 0f;
        
        while (timer < duration)
        {
            timer += Time.deltaTime;
            _currentLength = Mathf.Clamp01(timer / duration);
            UpdateLine();
            yield return null;
        }

        _currentLength = 1f;
    }

    public IEnumerator Idle(float duration)
    {
        _currentLength = 1f;
        UpdateLine();
        yield return new WaitForSeconds(duration);
    }

    public IEnumerator Close(float duration)
    {
        var timer = 0f;
        
        while (timer < duration)
        {
            timer += Time.deltaTime;
            _currentLength = 1f - Mathf.Clamp01(timer / duration);
            UpdateLine();
            yield return null;
        }

        _currentLength = 0f;
        _lr.enabled = false;
    }
    #endregion

    #region * Animation Event Functions
    private void EnableTrailRenderer() => trailRenderer.emitting = true;
    private void DisableTrailRenderer() => trailRenderer.emitting = false;
    #endregion
}
