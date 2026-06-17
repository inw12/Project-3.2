public interface IDamageable
{
    HealthContext Health { get; }

    void DecreaseHealth(float amount);
    void IncreaseHealth(float amount);
}
