using System.Collections;
public interface IHitstunnable
{
    float TimeScale     { get; }
    bool InHitstun      { get; }

    IEnumerator TriggerHitstun(float duration);
}