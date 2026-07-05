public class IceEffect : IDebuff
{
    private readonly float _slow;

    public float Duration { get; set; }
    public int Stacks { get; set; } = 1;

    public IceEffect(float duration = 2f, float slow = 0.3f)
    {
        Duration = duration;
        _slow = slow;
    }

    public void OnApply(Monster monster)
    {
        float slow = _slow * StackMultiplier();
        monster.MoveSpeedMultiplier = Mathf.Max(1f - slow, 0f);
        monster.IceParticle.Visible = true;
    }

    public void OnTick(Monster monster, float delta)
    {
    }

    public void OnExpire(Monster monster)
    {
        monster.MoveSpeedMultiplier = 1f;
        monster.IceParticle.Visible = false;
    }

    private float StackMultiplier() => 1f + (Stacks - 1) * 0.1f;
}
