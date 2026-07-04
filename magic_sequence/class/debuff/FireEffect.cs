public class FireEffect : IDebuff
{
    private const float TickInterval = 0.5f;

    private readonly int _tickDamage;
    private float _tickTimer;

    public float Duration { get; set; }
    public int Stacks { get; set; } = 1;

    public FireEffect(float duration = 2f, int tickDamage = 40)
    {
        Duration = duration;
        _tickDamage = tickDamage;
    }

    public void OnApply(Monster monster)
    {
    }

    public void OnTick(Monster monster, float delta)
    {
        _tickTimer += delta;
        if (_tickTimer < TickInterval)
            return;

        _tickTimer -= TickInterval;
        int damage = Mathf.RoundToInt(_tickDamage * StackMultiplier());
        monster.TakeDamage(damage, ColorPreset.Red);
    }

    public void OnExpire(Monster monster)
    {
    }

    private float StackMultiplier() => 1f + (Stacks - 1) * 0.1f;
}
