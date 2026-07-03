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

    public static Wand[] GetWandDrops(WandPool pool, int count)
    {
        var filters = new Func<Wand, bool>[count];
        for (int i = 0; i < count; i++)
        {
            Tier tier = RollTier();
            filters[i] = w => w.Tier == tier;
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
}
