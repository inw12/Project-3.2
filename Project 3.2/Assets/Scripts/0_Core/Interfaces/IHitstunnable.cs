public interface IHitstunnable
{
    float TimeScale     { get; }
    bool InHitstun      { get; }

    void TriggerHitstun(float duration);
}