using Godot;
using System.Collections.Generic;

public static class DropUtil
{

    private static readonly RandomNumberGenerator _rng = new();

    static DropUtil() => _rng.Randomize();

    public static Magic[] GetMagicDrops(MagicPool pool, int count, EnhanceData enhanceData)
    {
        var filters = new Func<Magic, bool>[count];
        for (int i = 0; i < count; i++)
        {
            Tier tier = RollTier(enhanceData);
            filters[i] = i == 0
                ? m => m.Tier == tier && m.MagicEffect is MagicSpell
                : m => m.Tier == tier;
        }
        return pool.DrawFixedSlots(filters);
    }

    public static Wand[] GetWandDrops(WandPool pool, int count)
    {
        Wand[] ownedWands = Blackboard.Wands ?? Array.Empty<Wand>();
        HashSet<string> ownedIds = ownedWands
            .SelectMany(GetWandIds)
            .Where(id => !string.IsNullOrEmpty(id))
            .ToHashSet();

        var filters = new Func<Wand, bool>[count];
        for (int i = 0; i < count; i++)
        {
            filters[i] = w => !GetWandIds(w).Any(ownedIds.Contains);
        }
        return pool.DrawFixedSlots(filters);
    }

    private static IEnumerable<string> GetWandIds(Wand wand)
    {
        if (wand == null)
            yield break;

        if (!string.IsNullOrEmpty(wand.ResourcePath))
            yield return System.IO.Path.GetFileNameWithoutExtension(wand.ResourcePath);

        if (!string.IsNullOrEmpty(wand.WandName))
            yield return wand.WandName;
    }

    private static Tier RollTier(EnhanceData enhanceData)
    {
        if (enhanceData == null) return Tier.Epic;

        float tier1Weight = Mathf.Max(0, enhanceData.Tier1Weight);
        float tier2Weight = Mathf.Max(0, enhanceData.Tier2Weight);
        float tier3Weight = Mathf.Max(0, enhanceData.Tier3Weight);
        float totalWeight = tier1Weight + tier2Weight + tier3Weight;

        if (totalWeight <= 0) return Tier.Rare;

        float roll = _rng.Randf() * totalWeight;
        if (roll < tier1Weight) return Tier.Rare;
        if (roll < tier1Weight + tier2Weight) return Tier.Epic;
        return Tier.Legendary;
    }
}
