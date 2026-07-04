using System.Collections.Generic;

[GlobalClass]
public partial class MagicPerkWaveform : MagicPerk
{
    [Export] public float TickIntervalSeconds { get; private set; } = 0.3f;
    [Export] public float DamageMultiplier { get; private set; } = 1f;
    [Export] public float EnhancedDamageMultiplier { get; private set; } = 1f;
    [Export] public float DurationFrameMultiplier { get; private set; } = 1f;
    [Export] public float EnhancedDurationFrameMultiplier { get; private set; } = 1.5f;

    private readonly Dictionary<ulong, float> _timers = new();

    public override void ApplyToNode(MagicNode node)
    {
        if (node == null)
            return;

        MagicStat stat = node.Stat;
        stat.DurationFrame = Mathf.Max(0, Mathf.RoundToInt(stat.DurationFrame * CurrentDurationFrameMultiplier));
        node.Stat = stat;
    }

    public override void OnNodeMove(MagicNode node, List<Monster> targets, float delta)
    {
        if (node?.MagicSpell == null || targets.Count == 0)
            return;

        ulong nodeId = node.GetInstanceId();
        _timers.TryGetValue(nodeId, out float timer);
        timer += delta;

        if (timer < TickIntervalSeconds)
        {
            _timers[nodeId] = timer;
            return;
        }

        _timers[nodeId] = 0f;

        foreach (Monster monster in targets)
        {
            HitInfo hit = MagicCombo.BuildHit(node, node.MagicSpell.Elemental, monster);
            hit.Damage = Mathf.Max(0, Mathf.RoundToInt(hit.Damage * CurrentDamageMultiplier));
            monster.Hit(hit);
        }
    }

    public override void MagicEnhance()
    {
        IsEnhanced = true;
    }

    private float CurrentDamageMultiplier => IsEnhanced ? EnhancedDamageMultiplier : DamageMultiplier;
    private float CurrentDurationFrameMultiplier => IsEnhanced ? EnhancedDurationFrameMultiplier : DurationFrameMultiplier;
}
