using Godot.Collections;
using System.Collections.Generic;

public partial class WandNode : Control
{
    public Wand Wand { get; private set; }

    private int _loadedIndex;

    public void Setup(Wand wand)
    {
        Wand = wand;
        _loadedIndex = 0;
        Visible = wand != null;
    }

    public int GetChargedMagicIndex()
    {
        if (Wand == null || Wand.Magics == null || Wand.Magics.Count == 0)
            return -1;

        int count = Wand.Magics.Count;

        for (int attempt = 0; attempt < count; attempt++)
        {
            int index = (_loadedIndex + attempt) % count;

            if (Wand.Magics[index]?.MagicEffect is MagicSpell)
                return index;
        }

        return -1;
    }

    public Array<MagicNode> Active()
    {
        var result = new Array<MagicNode>();

        int index = GetChargedMagicIndex();

        if (index < 0)
            return result;

        int count = Wand.Magics.Count;
        Magic magic = Wand.Magics[index];
        MagicSpell spell = magic.MagicEffect as MagicSpell;

        MagicNode node = spell.MagicNodePack.Instantiate<MagicNode>();

        var perks = new List<MagicPerk>();
        if (Wand.WandPerk != null)
            perks.Add(Wand.WandPerk);

        node.Setup(spell, perks);
        result.Add(node);

        _loadedIndex = (index + 1) % count;

        return result;
    }
}
