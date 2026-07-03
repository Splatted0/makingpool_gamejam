using System.Collections.Generic;

[GlobalClass]
public partial class BasicProjectileSpell : MagicSpell
{
    public override void SpawnEffect(MagicNode node)
    {
    }

    public override void MoveEffect(MagicNode node, List<Monster> targets, float fdelta)
    {
        foreach (Monster monster in targets)
        {
            monster.Hit(node.GetHitInfo());
            node.QueueFree();
            return;
        }
    }

    public override void ArrivalEffect(MagicNode node, List<Monster> targetMonster, int progressedFrame)
    {
        node.QueueFree();
    }
}
