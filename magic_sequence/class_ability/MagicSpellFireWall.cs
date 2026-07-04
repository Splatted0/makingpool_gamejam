using System.Collections.Generic;

[GlobalClass]
public partial class MagicSpellFireWall : MagicSpell
{
    public override void SpawnEffect(MagicNode node) { }

    public override void MoveEffect(MagicNode node, List<Monster> targets, float fdelta) { }
    
    public override void ArrivalEffect(MagicNode node, List<Monster> targetMonster, int progressedFrame)
    {
        foreach (var monster in targetMonster)
        {
            monster.Hit(node.GetHitInfo());
        }
    }
}