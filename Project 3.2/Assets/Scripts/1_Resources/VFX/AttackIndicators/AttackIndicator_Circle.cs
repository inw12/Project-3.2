using UnityEngine;

public class AttackIndicator_Circle : AttackIndicator
{
    private static readonly int FillProgress = Shader.PropertyToID("_FillProgress");

    protected override void SetFillProgress(float progress)
    {
        fillRenderer.GetPropertyBlock(_mpb);
        _mpb.SetFloat(FillProgress, progress);
        fillRenderer.SetPropertyBlock(_mpb);
    }
}