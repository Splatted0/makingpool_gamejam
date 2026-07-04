using System.Collections.Generic;

public partial class MagicPerkKnuckBack: MagicPerkArrival
{
    [Export] public float KnockbackValue = 50;
    [Export] public float EnhancedValue = 100;
    public override void ArrivalEffect(MagicSpell magicSpell, List<Monster> targets, int progressedFrame)
    {
        foreach (var monster in targets)
        {
            monster.Knockback(KnockbackValue);
        }
    }

    public override void MagicEnhance()
    {
        KnockbackValue = EnhancedValue;
        IsEnhanced = true;
    }
}