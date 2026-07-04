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

        int startIndex = GetChargedMagicIndex();

        if (startIndex < 0)
            return result;

        int count = Wand.Magics.Count;

        for (int offset = 0; offset < count; offset++)
        {
            int index = (startIndex + offset) % count;

            if (Wand.Magics[index]?.MagicEffect is not MagicSpell spell)
                continue;

            MagicNode node = spell.MagicNodePack.Instantiate<MagicNode>();

            List<MagicPerk> perks = CollectQueuedPerks(index, count);
            if (Wand.WandPerk != null)
                perks.Add(Wand.WandPerk);

            node.Setup(spell, perks);
            result.Add(node);
        }

        _loadedIndex = startIndex;

        return result;
    }

    private List<MagicPerk> CollectQueuedPerks(int spellIndex, int count)
    {
        var perks = new List<MagicPerk>();

        for (int offset = 1; offset < count; offset++)
        {
            int index = (spellIndex + offset) % count;

            if (Wand.Magics[index]?.MagicEffect is MagicSpell)
                break;

            if (Wand.Magics[index]?.MagicEffect is MagicPerk perk)
                perks.Add(perk);
        }

        return perks;
    }
}
