using System.Collections.Generic;

public abstract partial class MagicPerkArrival : MagicPerk
{
    public abstract void ArrivalEffect(MagicSpell magicSpell, List<Monster> targets, int  progressedFrame);
}
