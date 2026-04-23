using UnityEngine;
public class TrainingDummy : MonoBehaviour
{
    [Header("Enemy Components")]
    [SerializeField] private EnemyHurtbox hurtbox;

    [Header("Stats")]
    [SerializeField] private float health = 100f;
    [SerializeField] private float speed = 10;

    void Awake()
    {
        if (hurtbox) hurtbox.DeathTriggered += OnDeath;
    }

    void Start()
    {
        hurtbox.Initialize(health);
    }

    void Update()
    {
        
    }

    void LateUpdate()
    {
        var deltaTime = Time.deltaTime;
        
        hurtbox.UpdateHurtbox(deltaTime);
    }

    void FixedUpdate()
    {
        
    }

    private void OnDeath()
    {
        
    }

    void OnDestroy()
    {
        if (hurtbox) hurtbox.DeathTriggered -= OnDeath;
    }
}