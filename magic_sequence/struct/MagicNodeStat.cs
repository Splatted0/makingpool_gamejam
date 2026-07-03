public struct MagicStat
{
    public float Speed;
    public float MaxDistance;
    public float Range;
    public int Damage;
    public int DurationFrame;

    public static MagicStat From(MagicSpell spell) => new MagicStat
    {
        Speed       = spell.BaseSpeed,
        MaxDistance = spell.BaseMaxDistance,
        Range       = spell.BaseRange,
        Damage      = spell.BaseDamage,
        DurationFrame = spell.BaseDurationFrame,
    };
}
