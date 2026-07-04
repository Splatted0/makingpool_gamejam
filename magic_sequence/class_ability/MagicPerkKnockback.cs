using System.Collections.Generic;

[GlobalClass]
public partial class MagicPerkKnockback : MagicPerk
{
    [Export] public float KnockbackValue { get; private set; } = 50f;
    [Export] public float EnhancedKnockbackValue { get; private set; } = 75f;

    public override void OnNodeArrival(MagicNode node, List<Monster> targets, int progressedFrame)
    {
        if (progressedFrame != 0)
            return;

        foreach (Monster monster in targets)
            monster.Knockback(CurrentKnockbackValue);
    }

    public override void MagicEnhance()
    {
        IsEnhanced = true;
    }

    private float CurrentKnockbackValue => IsEnhanced ? EnhancedKnockbackValue : KnockbackValue;
}
