using System.Collections.Generic;

[GlobalClass]
public partial class MagicSpellFreezingSphere : MagicSpell
{
    [Export] public float SlowAmount { get; private set; } = 0.3f;
    [Export] public float EnhancedSlowAmount { get; private set; } = 0.4f;
    [Export] public float SlowDuration { get; private set; } = 2f;

    public override void SpawnEffect(MagicNode node)
    {
    }

    public override void MoveEffect(MagicNode node, List<Monster> targets, float fdelta)
    {
        if (targets.Count == 0)
            return;

        node.PrimaryTarget ??= targets[0];
        node.TriggerArrival();
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
