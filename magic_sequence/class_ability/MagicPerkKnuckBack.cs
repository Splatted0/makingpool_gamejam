using System.Collections.Generic;

public partial class MagicPerkKnuckBack: MagicPerkArrival
{
    [Export] private float _knockbackVaule = 50;
    public override void ArrivalEffect(MagicSpell magicSpell, List<Monster> targets, int progressedFrame)
    {
        foreach (var monster in targets)
        {
            monster.Knockback(_knockbackVaule);
        }
    }
}