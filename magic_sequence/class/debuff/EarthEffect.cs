public class EarthEffect : IDebuff
{
    public float Duration { get; set; }
    public int Stacks { get; set; } = 1;

    public EarthEffect(float duration = 1f)
    {
        Duration = duration;
    }

    public void OnApply(Monster monster)
    {
        monster.IsStunned = true;
    }

    public void OnTick(Monster monster, float delta)
    {
    }

    public void OnExpire(Monster monster)
    {
        monster.IsStunned = false;
    }
}
