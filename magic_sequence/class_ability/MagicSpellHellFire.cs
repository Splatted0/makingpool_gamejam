using System.Collections.Generic;

[GlobalClass]
public partial class MagicSpellHellFire : MagicSpell
{
    private const int TickIntervalFrame = 30;

    [Export] public float LingerDamageRatio { get; private set; } = 0.2f;
    [Export] public float EnhancedLingerDamageRatio { get; private set; } = 0.25f;
    [Export] public float HealBlockDuration { get; private set; } = 2f;

    public override void SpawnEffect(MagicNode node)
    {
    }

    public override void MoveEffect(MagicNode node, List<Monster> targets, float fdelta)
    {
        if (targets.Count == 0)
            return;

        if (node.ForcePierce)
        {
            foreach (Monster monster in targets)
            {
                if (node.TryMarkMoveHit(monster))
                    monster.Hit(BuildHit(node, monster));
            }
            return;
        }

        node.PrimaryTarget ??= targets[0];
        node.TriggerArrival();
    }

    public override void ArrivalEffect(MagicNode node, List<Monster> targets, int progressedFrame)
    {
        if (progressedFrame == 0)
        {
            foreach (Monster monster in targets)
                monster.Hit(BuildHit(node, monster));
            return;
        }

        if (progressedFrame % TickIntervalFrame != 0)
            return;

        int damage = Mathf.RoundToInt(node.Stat.Damage * CurrentLingerDamageRatio());
        foreach (Monster monster in targets)
        {
            monster.Hit(new HitInfo
            {
                Damage = damage,
                SourceTeam = Team.Player,
                Element = Elemental.Fire,
                SuppressElementEffect = true,
                ApplyHealBlockEffect = true,
                HealBlockDurationSeconds = HealBlockDuration
            });
        }
    }

    public override void MagicEnhance()
    {
        IsEnhanced = true;
    }

    private HitInfo BuildHit(MagicNode node, Monster target)
    {
        HitInfo hit = MagicCombo.BuildHit(node, Elemental.Fire, target);
        hit.ApplyFireEffect = true;
        hit.ApplyHealBlockEffect = true;
        hit.HealBlockDurationSeconds = HealBlockDuration;
        return hit;
    }

    private float CurrentLingerDamageRatio() => IsEnhanced ? EnhancedLingerDamageRatio : LingerDamageRatio;
}
