
using System.Collections.Generic;
using Godot.Collections;

public partial class WandNode : Control
{
    public Wand Wand { get; set; }

    public Array<MagicNode> Active()
    {
        var result = new Array<MagicNode>();
        if (Wand == null) return result;

        MagicSpell pendingSpell = null;
        MagicNode pendingNode = null;
        var pendingPerks = new List<MagicPerk>();

        foreach (Magic magic in Wand.Magics)
        {
            if (magic == null) continue;

            if (magic.MagicEffect is MagicSpell spell)
            {
                FlushPending(ref pendingSpell, ref pendingNode, pendingPerks, result);
                pendingSpell = spell;
                pendingNode = spell.MagicNodePack.Instantiate<MagicNode>();
                pendingPerks.Clear();
                if (Wand.WandPerk != null)
                    pendingPerks.Add(Wand.WandPerk);
            }
            else if (magic.MagicEffect is MagicPerk perk && pendingNode != null)
            {
                pendingPerks.Add(perk);
            }
        }

        FlushPending(ref pendingSpell, ref pendingNode, pendingPerks, result);
        return result;
    }

    private static void FlushPending(
        ref MagicSpell spell,
        ref MagicNode node,
        List<MagicPerk> perks,
        Array<MagicNode> result)
    {
        if (node == null) return;
        node.Setup(spell, perks);
        result.Add(node);
        spell = null;
        node = null;
    }
}
