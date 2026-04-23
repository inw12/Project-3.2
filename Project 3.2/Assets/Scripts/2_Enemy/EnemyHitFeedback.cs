using UnityEngine;
public class EnemyHitFeedback : MonoBehaviour
{
    [SerializeField] private Transform enemyModel;

    [Header("Emission")]
    [SerializeField] private Color hitColor;
    [SerializeField] private float hitBrightness;
    [SerializeField] private float effectSpeed;
    private Color _defaultColor;
    private Material _material;

    public void Initialize()
    {
        // Emission
        if (enemyModel.TryGetComponent(out SkinnedMeshRenderer mesh))
        {
            _material = mesh.material;
        }
        _material.EnableKeyword("_EMISSION");
        _defaultColor = new(0, 0, 0);
    }

    public void UpdateEnemyModel(float deltaTime)
    {
        Color current = _material.GetColor("_EmissionColor");

        Color next = Color.Lerp
        (
            current, 
            _defaultColor, 
            1f - Mathf.Exp(-effectSpeed * deltaTime)
        );

        _material.SetColor("_EmissionColor", next);
    }

    public void TriggerHitFeedback()
    {
        _material.SetColor("_EmissionColor", hitColor * hitBrightness);
    }
}
