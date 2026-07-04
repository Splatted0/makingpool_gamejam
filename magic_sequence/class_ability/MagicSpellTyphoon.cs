using System.Collections.Generic;

[GlobalClass]
public partial class MagicSpellTyphoon : MagicSpell
{
    [Export] public float KnockbackValue { get; private set; } = 100f;
    [Export] public int KnockbackIntervalFrame { get; private set; } = 60;
    [Export] public int EnhancedKnockbackIntervalFrame { get; private set; } = 30;

    private readonly Dictionary<ulong, float> _knockbackTimers = new();

    public override void SpawnEffect(MagicNode node)
    {
    }

    public override void MoveEffect(MagicNode node, List<Monster> targets, float fdelta)
    {
        foreach (Monster monster in targets)
        {
            if (node.TryMarkMoveHit(monster))
                monster.Hit(MagicCombo.BuildHit(node, Elemental.Wind, monster));
        }

        TickKnockback(node, targets, fdelta);
    }

    public override void ArrivalEffect(MagicNode node, List<Monster> targets, int progressedFrame)
    {
        int intervalFrame = IsEnhanced ? EnhancedKnockbackIntervalFrame : KnockbackIntervalFrame;
        if (intervalFrame <= 0 || progressedFrame % intervalFrame != 0)
            return;

        foreach (Monster monster in targets)
            monster.Knockback(KnockbackValue);
    }

    public override void MagicEnhance()
    {
        IsEnhanced = true;
    }

    private void TickKnockback(MagicNode node, List<Monster> targets, float delta)
    {
        if (targets.Count == 0)
            return;

        ulong id = node.GetInstanceId();
        float interval = (IsEnhanced ? EnhancedKnockbackIntervalFrame : KnockbackIntervalFrame) / 60f;
        if (interval <= 0f)
            interval = 1f;

        _knockbackTimers.TryGetValue(id, out float elapsed);
        elapsed += delta;
        if (elapsed < interval)
        {
            _knockbackTimers[id] = elapsed;
            return;
        }

        _knockbackTimers[id] = 0f;
        foreach (Monster monster in targets)
            monster.Knockback(KnockbackValue);
    }
}
