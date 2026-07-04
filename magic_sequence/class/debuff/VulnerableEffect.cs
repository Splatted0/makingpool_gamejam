public class VulnerableEffect : IDebuff
{
    private readonly float _damageMultiplier;

    public float Duration { get; set; }
    public int Stacks { get; set; } = 1;

    public VulnerableEffect(float duration = 2f, float damageMultiplier = 1.1f)
    {
        Duration = duration;
        _damageMultiplier = damageMultiplier;
    }

    public void OnApply(Monster monster)
    {
        monster.DamageTakenMultiplier = _damageMultiplier;
    }

    public void OnTick(Monster monster, float delta)
    {
    }

    public void OnExpire(Monster monster)
    {
        monster.DamageTakenMultiplier = 1f;
    }
}
