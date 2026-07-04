using System.Collections.Generic;

[GlobalClass]
public partial class MagicSpellFireWall : MagicSpell
{
    [Export] public float EnhanceValue = 0;
    public override void SpawnEffect(MagicNode node) { }

    public override void MoveEffect(MagicNode node, List<Monster> targets, float fdelta) { }
    
    public override void ArrivalEffect(MagicNode node, List<Monster> targetMonster, int progressedFrame)
    {
        foreach (var monster in targetMonster)
        {
            monster.Hit(node.GetHitInfo());
        }
    }

    public override void MagicEnhance()
    {
        //특정 값 = EnhanceValue;
        IsEnhanced = true;
    }
}