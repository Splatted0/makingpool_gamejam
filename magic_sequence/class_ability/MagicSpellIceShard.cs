using System.Collections.Generic;

[GlobalClass]
public partial class MagicSpellIceShard : MagicSpell
{
    [Export] public float SlowAmount { get; private set; } = 0.2f;
    [Export] public float SlowDuration { get; private set; } = 2f;

    public override void SpawnEffect(MagicNode node)
    {
    }

    public override void MoveEffect(MagicNode node, List<Monster> targets, float fdelta)
    {
        TryApplyProjectileSplit(node);

        if (targets.Count == 0)
            return;

        if (node.ForcePierce)
        {
            foreach (Monster monster in targets)
            {
                if (node.TryMarkMoveHit(monster))
                    monster.Hit(BuildHit(node, monster));
            }
            return;
        }

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
        hit.IceSlow = SlowAmount;
        hit.IceDurationSeconds = SlowDuration;
        return hit;
    }

    private static void TryApplyProjectileSplit(MagicNode node)
    {
        if (node.HasSplit || !MagicCombo.ShouldSplitProjectile(node, Elemental.Ice))
            return;

        node.HasSplit = true;
        node.SpawnSibling(-15f);
        node.SpawnSibling(15f);
    }
}
