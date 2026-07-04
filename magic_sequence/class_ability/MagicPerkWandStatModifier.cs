using Godot.Collections;

[GlobalClass]
public partial class MagicPerkWandStatModifier : MagicPerk
{
    [Export] public float DamageMultiplier { get; private set; } = 1f;
    [Export] public float EnhancedDamageMultiplier { get; private set; } = 1f;
    [Export] public float ProjectileDamageMultiplier { get; private set; } = 1f;
    [Export] public float EnhancedProjectileDamageMultiplier { get; private set; } = 1f;
    [Export] public float SpeedMultiplier { get; private set; } = 1f;
    [Export] public float EnhancedSpeedMultiplier { get; private set; } = 1f;
    [Export] public float MaxDistanceMultiplier { get; private set; } = 1f;
    [Export] public float EnhancedMaxDistanceMultiplier { get; private set; } = 1f;
    [Export] public float MoveRangeMultiplier { get; private set; } = 1f;
    [Export] public float EnhancedMoveRangeMultiplier { get; private set; } = 1f;
    [Export] public float ArrivalRangeMultiplier { get; private set; } = 1f;
    [Export] public float EnhancedArrivalRangeMultiplier { get; private set; } = 1f;
    [Export] public float DurationFrameMultiplier { get; private set; } = 1f;
    [Export] public float EnhancedDurationFrameMultiplier { get; private set; } = 1f;
    [Export] public int DurationFrameBonus { get; private set; }
    [Export] public int EnhancedDurationFrameBonus { get; private set; }
    [Export] public int ExtraFanProjectiles { get; private set; }
    [Export] public int EnhancedExtraFanProjectiles { get; private set; } = -1;
    [Export] public float FanAngleDegrees { get; private set; } = 15f;
    [Export] public float EnhancedFanAngleDegrees { get; private set; } = -1f;
    [Export] public bool ForcePierce { get; private set; }
    [Export] public bool SuppressWindElementEffects { get; private set; }
    [Export] public float FireCooldownMultiplier { get; private set; } = 1f;
    [Export] public float EnhancedFireCooldownMultiplier { get; private set; } = 1f;
    [Export] public float SlotDelayMultiplier { get; private set; } = 1f;
    [Export] public float EnhancedSlotDelayMultiplier { get; private set; } = 1f;
    [Export] public Array<float> SlotDamageMultipliers { get; private set; } = new();
    [Export] public float NearCoreDamageMultiplier { get; private set; } = 1f;
    [Export] public float EnhancedNearCoreDamageMultiplier { get; private set; } = 1f;
    [Export] public float NearCoreScreenWidthRatio { get; private set; } = 0.1f;

    public override void ApplyToNode(MagicNode node)
    {
        if (node?.MagicSpell == null)
            return;

        MagicStat stat = node.Stat;

        float damageMultiplier = CurrentDamageMultiplier;
        if (IsProjectileSpell(node.MagicSpell))
            damageMultiplier *= CurrentProjectileDamageMultiplier;

        if (node.CastSlotIndex >= 0 && node.CastSlotIndex < SlotDamageMultipliers.Count)
            damageMultiplier *= SlotDamageMultipliers[node.CastSlotIndex];

        if (IsNearCore(node))
            damageMultiplier *= CurrentNearCoreDamageMultiplier;

        stat.Damage = Mathf.Max(0, Mathf.RoundToInt(stat.Damage * damageMultiplier));
        stat.Speed *= CurrentSpeedMultiplier;
        stat.MaxDistance *= CurrentMaxDistanceMultiplier;
        stat.MoveRange *= CurrentMoveRangeMultiplier;
        stat.ArrivalRange *= CurrentArrivalRangeMultiplier;
        stat.DurationFrame = Mathf.Max(0, Mathf.RoundToInt(stat.DurationFrame * CurrentDurationFrameMultiplier) + CurrentDurationFrameBonus);

        node.Stat = stat;
        node.ForcePierce = node.ForcePierce || ForcePierce;
        node.SuppressWindElementEffects = node.SuppressWindElementEffects || SuppressWindElementEffects;
    }

    public override void OnNodeSpawned(MagicNode node)
    {
        int extraFanProjectiles = CurrentExtraFanProjectiles;
        if (node == null || extraFanProjectiles <= 0 || node.HasSplit || !IsProjectileSpell(node.MagicSpell))
            return;

        node.HasSplit = true;

        int pairs = extraFanProjectiles / 2;
        for (int i = 1; i <= pairs; i++)
        {
            float angle = CurrentFanAngleDegrees * i;
            node.SpawnSibling(-angle);
            node.SpawnSibling(angle);
        }

        if (extraFanProjectiles % 2 == 1)
            node.SpawnSibling(0f);
    }

    public override float GetFireCooldownMultiplier() => CurrentFireCooldownMultiplier;

    public override float GetSlotDelayMultiplier() => CurrentSlotDelayMultiplier;

    public override void MagicEnhance()
    {
        IsEnhanced = true;
    }

    private float CurrentDamageMultiplier => IsEnhanced ? EnhancedDamageMultiplier : DamageMultiplier;
    private float CurrentProjectileDamageMultiplier => IsEnhanced ? EnhancedProjectileDamageMultiplier : ProjectileDamageMultiplier;
    private float CurrentSpeedMultiplier => IsEnhanced ? EnhancedSpeedMultiplier : SpeedMultiplier;
    private float CurrentMaxDistanceMultiplier => IsEnhanced ? EnhancedMaxDistanceMultiplier : MaxDistanceMultiplier;
    private float CurrentMoveRangeMultiplier => IsEnhanced ? EnhancedMoveRangeMultiplier : MoveRangeMultiplier;
    private float CurrentArrivalRangeMultiplier => IsEnhanced ? EnhancedArrivalRangeMultiplier : ArrivalRangeMultiplier;
    private float CurrentDurationFrameMultiplier => IsEnhanced ? EnhancedDurationFrameMultiplier : DurationFrameMultiplier;
    private int CurrentDurationFrameBonus => IsEnhanced ? EnhancedDurationFrameBonus : DurationFrameBonus;
    private int CurrentExtraFanProjectiles => IsEnhanced && EnhancedExtraFanProjectiles >= 0 ? EnhancedExtraFanProjectiles : ExtraFanProjectiles;
    private float CurrentFanAngleDegrees => IsEnhanced && EnhancedFanAngleDegrees >= 0f ? EnhancedFanAngleDegrees : FanAngleDegrees;
    private float CurrentFireCooldownMultiplier => IsEnhanced ? EnhancedFireCooldownMultiplier : FireCooldownMultiplier;
    private float CurrentSlotDelayMultiplier => IsEnhanced ? EnhancedSlotDelayMultiplier : SlotDelayMultiplier;
    private float CurrentNearCoreDamageMultiplier => IsEnhanced ? EnhancedNearCoreDamageMultiplier : NearCoreDamageMultiplier;

    private static bool IsProjectileSpell(MagicSpell spell) => spell is not MagicSpellFireWall;

    private bool IsNearCore(MagicNode node)
    {
        if (CurrentNearCoreDamageMultiplier == 1f)
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
