using System.Collections.Generic;

[GlobalClass]
public partial class MagicSpellWindArrow : MagicSpell
{
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
    }

    public override void ArrivalEffect(MagicNode node, List<Monster> targets, int progressedFrame)
    {
    }

    public override void MagicEnhance()
    {
        IsEnhanced = true;
    }
}
