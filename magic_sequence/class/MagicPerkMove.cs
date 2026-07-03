using System.Collections.Generic;

public abstract partial class MagicPerkMove : MagicPerk
{
    public abstract void MoveEffect(float fdelta, MagicSpell magicSpell, List<Monster> targets);
}