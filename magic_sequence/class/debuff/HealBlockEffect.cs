public class HealBlockEffect : IDebuff
{
    public float Duration { get; set; }
    public int Stacks { get; set; } = 1;

    public HealBlockEffect(float duration = 2f)
    {
        Duration = duration;
    }

    public void OnApply(Monster monster)
    {
        monster.HealBlocked = true;
    }

    public void OnTick(Monster monster, float delta)
    {
    }

    public void OnExpire(Monster monster)
    {
        monster.HealBlocked = false;
    }
}
