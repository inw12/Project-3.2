using System;
using UnityEngine;
public abstract class AttackIndicator : MonoBehaviour
{
    // ** Delete Later **
    [Header("Debug Stuff")]
    [SerializeField] private float duration;

    [Space]
    [SerializeField] protected MeshRenderer fillRenderer;
    
    protected MaterialPropertyBlock _mpb;   
    protected float _duration;
    protected float _elapsed;
    private Action _onComplete;

    public void Initialize(float duration, Action onComplete)
    {
        _duration = duration;
        _elapsed = 0f;
        _onComplete = onComplete;
        _mpb = new MaterialPropertyBlock();

        OnInitialize();
    }

    // ** Delete Later **
    void Start()
    {
        _duration = duration;
        _elapsed = 0f;
        _mpb = new MaterialPropertyBlock();
    }
    // ******************

    void Update()
    {
        _elapsed += Time.deltaTime;
        float progress = Mathf.Clamp01(_elapsed / _duration);

        SetFillProgress(progress);

        if (_elapsed >= _duration)
        {
            _onComplete?.Invoke();
            Destroy(gameObject);
        }
    }

    // Called once on Initialize for any subclass-specific setup
    protected virtual void OnInitialize() { }

    // Each subclass drives its fill differently
    protected abstract void SetFillProgress(float progress);
}
