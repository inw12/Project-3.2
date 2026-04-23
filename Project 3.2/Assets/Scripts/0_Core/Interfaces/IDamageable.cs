public interface IDamageable
{
    float MaxHealth     { get; }
    float CurrentHealth { get; }

    void DecreaseHealth(float amount);
    void IncreaseHealth(float amount);
}
