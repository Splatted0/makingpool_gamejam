using System.Collections.Generic;

[GlobalClass]
public partial class MagicSpellFallingRock : MagicSpell
{
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
            monster.Hit(MagicCombo.BuildHit(node, Elemental.Earth, monster));
    }

    public override void MagicEnhance()
    {
        IsEnhanced = true;
    }
}
