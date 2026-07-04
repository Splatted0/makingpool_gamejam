using System.Collections.Generic;

[GlobalClass]
public partial class MagicSpellFireBullet : MagicSpell
{
    public override void SpawnEffect(MagicNode node)
    {
    }

    public override void MoveEffect(MagicNode node, List<Monster> targets, float fdelta)
    {
        if (targets.Count == 0)
            return;

        if (node.ForcePierce)
        {
            foreach (Monster monster in targets)
            {
                if (node.TryMarkMoveHit(monster))
                    monster.Hit(MagicCombo.BuildHit(node, Elemental.Fire, monster));
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
            monster.Hit(MagicCombo.BuildHit(node, Elemental.Fire, monster));
    }

    public override void MagicEnhance()
    {
        IsEnhanced = true;
    }
}
