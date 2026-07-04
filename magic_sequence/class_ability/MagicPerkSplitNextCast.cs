using System.Collections.Generic;

[GlobalClass]
public partial class MagicPerkSplitNextCast : MagicPerk
{
    [Export] public float SplitAngleDegrees { get; private set; } = 15f;

    public void SpawnEffect(MagicNode node)
    {
        if (node.HasSplit)
            return;

        node.HasSplit = true;
        node.SpawnSibling(-SplitAngleDegrees);
        node.SpawnSibling(SplitAngleDegrees);
    }
}
