public struct MagicStat
{
    public float Speed;
    public float MaxDistance;
    public float MoveRange;
    public float ArrivalRange;
    public int Damage;
    public int DurationFrame;

    public static MagicStat From(MagicSpell spell) => new MagicStat
    {
        Speed       = spell.BaseSpeed,
        MaxDistance = spell.BaseMaxDistance,
        MoveRange       = spell.BaseMoveRange,
        ArrivalRange = spell.BaseArrivalRange,
        Damage      = spell.BaseDamage,
        DurationFrame = spell.BaseDurationFrame,
    };
}
