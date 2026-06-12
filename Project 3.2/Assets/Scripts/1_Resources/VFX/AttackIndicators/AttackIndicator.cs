using System;
using UnityEngine;
[RequireComponent(typeof(MeshRenderer))]
public abstract class AttackIndicator : MonoBehaviour
{
    // Unity Components
    protected MeshRenderer _mr;
    protected MaterialPropertyBlock _mpb;   

    // Timers
    protected float _duration;
    protected float _elapsed;

    // Event/Action Signal
    private Action _onComplete;


    public void Initialize(float duration, Action onComplete, Vector3 scale)
    {
        _mr = GetComponent<MeshRenderer>();
        _mpb = new MaterialPropertyBlock();

        _duration = duration;
        _elapsed = 0f;

        transform.localScale = scale;

        _onComplete = onComplete;
    }

    void Update()
    {
        // - Update duration progress
        _elapsed += Time.deltaTime;
        float progress = Mathf.Clamp01(_elapsed / _duration);

        // - Update Shader Graph
        SetFillProgress(progress);

        // - END of Attack Indicator
        if (_elapsed >= _duration)
        {
            _onComplete?.Invoke();
            Destroy(gameObject);
        }
    }

    protected abstract void SetFillProgress(float progress);
}
