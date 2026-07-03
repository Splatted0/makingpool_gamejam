
using System.Collections.Generic;

public partial class MagicSpellFireWall : MagicSpell
{
    public override void SpawnEffect(MagicNode node)
    {
        throw new NotImplementedException();
    }

    public override void ArrivalEffect(MagicNode node, List<Monster> targetMonster, float fdelta)
    {
        foreach (var monster in targetMonster)
        {
            monster.Hit(node.GetHitInfo());
        }
    }

    public override void MoveEffect(MagicNode node, List<Monster> targets, float fdelta)
    {
        throw new NotImplementedException();
    }
}