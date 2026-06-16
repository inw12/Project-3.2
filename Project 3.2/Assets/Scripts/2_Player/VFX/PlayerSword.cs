using System.Collections;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PlayerSword : MonoBehaviour
{
    [SerializeField] private Transform spawn;
    [SerializeField] private Vector2 swordDimensions;   // x: width, y: length

    private LineRenderer _lr;
    private float _currentLength;

    void Awake()
    {
        _lr = GetComponent<LineRenderer>();
        _lr.enabled = false;

        _lr.positionCount = 2;
        _lr.startWidth = _lr.endWidth = swordDimensions.x;
    }

    void LateUpdate()
    {
        if (_lr.enabled)
            UpdateLine();
    }

    private void UpdateLine()
    {
        var origin = spawn.position;
        var tip = origin + spawn.forward * (swordDimensions.y * _currentLength);

        _lr.SetPosition(0, origin);
        _lr.SetPosition(1, tip);
    }

    #region * Public Access
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
}
