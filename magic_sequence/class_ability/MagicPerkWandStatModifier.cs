using Godot.Collections;

[GlobalClass]
public partial class MagicPerkWandStatModifier : MagicPerk
{
    [Export] public float DamageMultiplier { get; private set; } = 1f;
    [Export] public float ProjectileDamageMultiplier { get; private set; } = 1f;
    [Export] public float SpeedMultiplier { get; private set; } = 1f;
    [Export] public float MaxDistanceMultiplier { get; private set; } = 1f;
    [Export] public float MoveRangeMultiplier { get; private set; } = 1f;
    [Export] public float ArrivalRangeMultiplier { get; private set; } = 1f;
    [Export] public int DurationFrameBonus { get; private set; }
    [Export] public int ExtraFanProjectiles { get; private set; }
    [Export] public float FanAngleDegrees { get; private set; } = 15f;
    [Export] public bool ForcePierce { get; private set; }
    [Export] public bool SuppressWindElementEffects { get; private set; }
    [Export] public float SlotDelayMultiplier { get; private set; } = 1f;
    [Export] public Array<float> SlotDamageMultipliers { get; private set; } = new();
    [Export] public float NearCoreDamageMultiplier { get; private set; } = 1f;
    [Export] public float NearCoreScreenWidthRatio { get; private set; } = 0.1f;

    public override void ApplyToNode(MagicNode node)
    {
        if (node?.MagicSpell == null)
            return;

        MagicStat stat = node.Stat;

        float damageMultiplier = DamageMultiplier;
        if (IsProjectileSpell(node.MagicSpell))
            damageMultiplier *= ProjectileDamageMultiplier;

        if (node.CastSlotIndex >= 0 && node.CastSlotIndex < SlotDamageMultipliers.Count)
            damageMultiplier *= SlotDamageMultipliers[node.CastSlotIndex];

        if (IsNearCore(node))
            damageMultiplier *= NearCoreDamageMultiplier;

        stat.Damage = Mathf.Max(0, Mathf.RoundToInt(stat.Damage * damageMultiplier));
        stat.Speed *= SpeedMultiplier;
        stat.MaxDistance *= MaxDistanceMultiplier;
        stat.MoveRange *= MoveRangeMultiplier;
        stat.ArrivalRange *= ArrivalRangeMultiplier;
        stat.DurationFrame = Mathf.Max(0, stat.DurationFrame + DurationFrameBonus);

        node.Stat = stat;
        node.ForcePierce = node.ForcePierce || ForcePierce;
        node.SuppressWindElementEffects = node.SuppressWindElementEffects || SuppressWindElementEffects;
    }

    public override void OnNodeSpawned(MagicNode node)
    {
        if (node == null || ExtraFanProjectiles <= 0 || node.HasSplit || !IsProjectileSpell(node.MagicSpell))
            return;

        node.HasSplit = true;

        int pairs = ExtraFanProjectiles / 2;
        for (int i = 1; i <= pairs; i++)
        {
            float angle = FanAngleDegrees * i;
            node.SpawnSibling(-angle);
            node.SpawnSibling(angle);
        }

        if (ExtraFanProjectiles % 2 == 1)
            node.SpawnSibling(0f);
    }

    public override float GetSlotDelayMultiplier() => SlotDelayMultiplier;

    public override void MagicEnhance()
    {
        IsEnhanced = true;
    }

    private static bool IsProjectileSpell(MagicSpell spell) => spell is not MagicSpellFireWall;

    private bool IsNearCore(MagicNode node)
    {
        if (NearCoreDamageMultiplier == 1f)
            return false;

        Core core = Blackboard.Core;
        if (core == null)
            return false;

        float threshold = GetViewportWidth(node) * NearCoreScreenWidthRatio;
        return node.GlobalPosition.X <= core.GlobalPosition.X + threshold;
    }

    private static float GetViewportWidth(Node node)
    {
        Viewport viewport = node.GetViewport();
        return viewport?.GetVisibleRect().Size.X ?? 0f;
    }
}
