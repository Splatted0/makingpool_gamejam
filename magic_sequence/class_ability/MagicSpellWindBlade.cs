using System.Collections.Generic;

[GlobalClass]
public partial class MagicSpellWindBlade : MagicSpell
{
    public override void SpawnEffect(MagicNode node)
    {
    }

    public override void MoveEffect(MagicNode node, List<Monster> targets, float fdelta)
    {
        bool pierces = MagicCombo.ShouldPierce(node, Elemental.Wind);

        foreach (Monster monster in targets)
        {
            if (!node.TryMarkMoveHit(monster))
                continue;

            monster.Hit(MagicCombo.BuildHit(node, Elemental.Wind, monster));

            if (!pierces)
            {
                node.PrimaryTarget ??= monster;
                node.TriggerArrival();
                return;
            }
        }
    }

    public override void ArrivalEffect(MagicNode node, List<Monster> targetMonster, int progressedFrame)
    {
        if (progressedFrame != 0)
            return;

        if (MagicCombo.ShouldPierce(node, Elemental.Wind))
            return;

        foreach (Monster monster in targetMonster)
        {
            if (!node.TryMarkMoveHit(monster))
                continue;

            monster.Hit(MagicCombo.BuildHit(node, Elemental.Wind, monster));
        }
    }
}
