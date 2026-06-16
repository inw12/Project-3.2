public struct HealthContext
{
    public float            CurrentHealth;
    public readonly float   MaxHealth;
    public bool             IsAlive;

    public HealthContext(float health)
    {
        CurrentHealth = MaxHealth = health;
        IsAlive = true;
    }
}