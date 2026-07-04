using System.Collections.Generic;

[GlobalClass]
public partial class MagicSpellAbsoluteZero : MagicSpell
{
    [Export] public float SlowAmount { get; private set; } = 0.5f;
    [Export] public float EnhancedSlowAmount { get; private set; } = 0.6f;
    [Export] public float SlowDuration { get; private set; } = 5f;

    public override void SpawnEffect(MagicNode node)
    {
        node.TriggerArrival();
    }

    public override void MoveEffect(MagicNode node, List<Monster> targets, float fdelta)
    {
    }

    public override void ArrivalEffect(MagicNode node, List<Monster> targets, int progressedFrame)
    {
        if (progressedFrame != 0)
            return;

        foreach (Monster monster in targets)
            monster.Hit(BuildHit(node, monster));
    }

    public override void MagicEnhance()
    {
        IsEnhanced = true;
    }

    private HitInfo BuildHit(MagicNode node, Monster target)
    {
        HitInfo hit = MagicCombo.BuildHit(node, Elemental.Ice, target);
        hit.IceSlow = IsEnhanced ? EnhancedSlowAmount : SlowAmount;
        hit.IceDurationSeconds = SlowDuration;
        return hit;
    }
}
