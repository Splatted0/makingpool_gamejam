using System.Collections.Generic;

[GlobalClass]
public partial class MagicSpellFireBullet : MagicSpell
{
    [Export] public float EnhanceValue = 0;
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

    public override void ArrivalEffect(MagicNode node, List<Monster> targetMonster, int progressedFrame)
    {
        if (progressedFrame != 0)
            return;

        foreach (Monster monster in targetMonster)
            monster.Hit(MagicCombo.BuildHit(node, Elemental.Fire, monster));
    }

    public override void MagicEnhance()
    {
        //특정 값 = EnhanceValue;
        IsEnhanced = true;
    }
}
