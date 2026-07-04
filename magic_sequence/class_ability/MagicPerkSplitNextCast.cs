using System.Collections.Generic;

[GlobalClass]
public partial class MagicPerkSplitNextCast : MagicPerk
{
    [Export] public float SplitAngleDegrees { get; private set; } = 15f;

    public void MoveEffect(MagicNode node, List<Monster> targets, float fdelta)
    {
        if (node.HasSplit)
            return;

        node.HasSplit = true;
        node.SpawnSibling(-SplitAngleDegrees);
        node.SpawnSibling(SplitAngleDegrees);
    }
}
