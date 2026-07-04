using System.Collections.Generic;

[GlobalClass]
public partial class MagicSpellIceShard : MagicSpell
{
    [Export] public float EnhanceValue;
    
    public override void SpawnEffect(MagicNode node)
    {
    }

    public override void MoveEffect(MagicNode node, List<Monster> targets, float fdelta)
    {
        TryApplyProjectileSplit(node);

        if (targets.Count == 0)
            return;

        node.PrimaryTarget ??= targets[0];
        node.TriggerArrival();
    }

    public override void ArrivalEffect(MagicNode node, List<Monster> targetMonster, int progressedFrame)
    {
        if (progressedFrame != 0)
            return;

        foreach (Monster monster in targetMonster)
            monster.Hit(MagicCombo.BuildHit(node, Elemental.Ice, monster));
    }

    private static void TryApplyProjectileSplit(MagicNode node)
    {
        if (node.HasSplit || !MagicCombo.ShouldSplitProjectile(node, Elemental.Ice))
            return;

        node.HasSplit = true;
        node.SpawnSibling(-15f);
        node.SpawnSibling(15f);
    }

    public override void MagicEnhance()
    {
        //특정 값 = EnhanceValue;
        IsEnhanced = true;
    }
}
