using UnityEngine;

[RequireComponent(typeof(SphereCollider), typeof(MeshRenderer))]
public class Shield : MonoBehaviour, IDamageable
{
    // Base Color
    [ColorUsage(hdr:true, showAlpha:true)]
    [SerializeField] private Color baseColor;

    // Damage Color
    [ColorUsage(hdr:true, showAlpha:true)]
    [SerializeField] private Color damageColor;
    [SerializeField] private float speed;

    [ColorUsage(hdr:true, showAlpha:true)]

    private static readonly int TextureColor = Shader.PropertyToID("_Texture_1_Color");

    // Shield Components
    private HealthContext _shieldHealth;
    private SphereCollider _collider;
    private MeshRenderer _mr;
    private MaterialPropertyBlock _mpb;

    void Start()
    {
        _collider = GetComponent<SphereCollider>();

        _mr = GetComponent<MeshRenderer>();
        _mpb = new MaterialPropertyBlock();

        _shieldHealth = new HealthContext(999f);
    } 

    void Update()
    {
        _mr.GetPropertyBlock(_mpb);

        if (_mpb.GetColor(TextureColor) != baseColor)
        {
            Color current = _mpb.GetColor(TextureColor);
            Color next = Color.Lerp
            (
                current,
                baseColor,
                1f - Mathf.Exp(-speed * Time.deltaTime)
            );
            _mpb.SetColor(TextureColor, next);
        }
        
        _mr.SetPropertyBlock(_mpb);
    }


    #region * 'IDamageable' Functions ------------------------------------
    public HealthContext Health => _shieldHealth;

    public void DecreaseHealth(float amount)
    {
        _mr.GetPropertyBlock(_mpb);
        _mpb.SetColor(TextureColor, damageColor);
        _mr.SetPropertyBlock(_mpb);
    }

    public void IncreaseHealth(float amount) {}
    #endregion
}
