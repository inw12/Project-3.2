using UnityEngine;
public class AttackIndicator_Circle : AttackIndicator
{
    private static readonly int FillProgress = Shader.PropertyToID("_FillProgress");

    protected override void SetFillProgress(float progress)
    {
        _mr.GetPropertyBlock(_mpb);
        _mpb.SetFloat(FillProgress, progress);
        _mr.SetPropertyBlock(_mpb);
    }
}