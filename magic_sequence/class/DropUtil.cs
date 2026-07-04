using Godot;

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
            filters[i] = m => m.Tier == tier;
        }
        return pool.DrawFixedSlots(filters);
    }

    public static Wand[] GetWandDrops(WandPool pool, int count)
    {
        Wand[] ownedWands = Blackboard.Wands;
        var filters = new Func<Wand, bool>[count];
        for (int i = 0; i < count; i++)
        {
            filters[i] = w => !ownedWands.Contains(w);
        }
        return pool.DrawFixedSlots(filters);
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
