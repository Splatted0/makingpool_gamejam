using Godot;

public static class DropUtil
{
    private const float ChanceLegendary = 0.03f;
    private const float ChanceEpic      = 0.12f;
    private const float ChanceRare      = 0.25f;

    private static readonly RandomNumberGenerator _rng = new();

    static DropUtil() => _rng.Randomize();

    public static Magic[] GetMagicDrops(MagicPool pool, int count)
    {
        var filters = new Func<Magic, bool>[count];
        for (int i = 0; i < count; i++)
        {
            Tier tier = RollTier();
            filters[i] = m => m.Tier == tier;
        }
        return pool.DrawFixedSlots(filters);
    }

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
        return pool.DrawFixedSlots(filters);
    }

    public static Wand[] GetWandDrops(WandPool pool, int count, EnhanceData enhanceData)
    {
        Wand[] ownedWands = Blackboard.Wands;
        var filters = new Func<Wand, bool>[count];
        for (int i = 0; i < count; i++)
        {
            Tier tier = RollTier(enhanceData);
            filters[i] = w => w.Tier == tier && System.Array.IndexOf(ownedWands, w) < 0;
        }
        return pool.DrawFixedSlots(filters);
    }

    private static Tier RollTier()
    {
        float roll = _rng.Randf();
        if (roll < ChanceLegendary)                              return Tier.Legendary;
        if (roll < ChanceLegendary + ChanceEpic)                return Tier.Epic;
        if (roll < ChanceLegendary + ChanceEpic + ChanceRare)   return Tier.Rare;
        return Tier.Common;
    }

    private static Tier RollTier(EnhanceData enhanceData)
    {
        if (enhanceData == null) return RollTier();

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
