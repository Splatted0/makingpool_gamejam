public struct MagicStat
{
    public float Speed;
    public float MaxDistance;
    public float Range;
    public float Damage;

    public static MagicStat From(MagicSpell spell) => new MagicStat
    {
        Speed       = spell.BaseSpeed,
        MaxDistance = spell.BaseMaxDistance,
        Range       = spell.BaseRange,
        Damage      = spell.BaseDamage,
    };
}
