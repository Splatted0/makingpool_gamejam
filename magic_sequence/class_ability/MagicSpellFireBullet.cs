using System.Collections.Generic;

[GlobalClass]
public partial class MagicSpellFireBullet: MagicSpell
{
    public override void SpawnEffect(MagicNode node)
    {
        throw new NotImplementedException();
    }
    public override void MoveEffect(MagicNode node, List<Monster> targets, float fdelta)
    {
        if (targets.Count == 0) {return;}
        
        node.TriggerArrival();
    }
    
    public override void ArrivalEffect(MagicNode node, List<Monster> targetMonster, int progressedFrame)
    {
        if (progressedFrame != 0) {return;}
        
        foreach (var monster in targetMonster)
        {
            monster.Hit(node.GetHitInfo());
        }
    }


}