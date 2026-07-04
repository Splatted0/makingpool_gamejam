using System.Collections.Generic;

[GlobalClass]
public partial class MagicSpellRockSpike : MagicSpell
{
    [Export] public float VulnerableDamageMultiplier { get; private set; } = 1.1f;
    [Export] public float EnhancedVulnerableDamageMultiplier { get; private set; } = 1.2f;
    [Export] public float VulnerableDuration { get; private set; } = 2f;

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
        if (progressedFrame != 0)
            return;

        foreach (Monster monster in targets)
            monster.Hit(BuildHit(node, monster));
    }

    public override void MagicEnhance()
    {
        IsEnhanced = true;
    }

    private HitInfo BuildHit(MagicNode node, Monster target)
    {
        HitInfo hit = MagicCombo.BuildHit(node, Elemental.Earth, target);
        hit.ApplyVulnerableEffect = true;
        hit.VulnerableDamageMultiplier = IsEnhanced ? EnhancedVulnerableDamageMultiplier : VulnerableDamageMultiplier;
        hit.VulnerableDurationSeconds = VulnerableDuration;
        return hit;
    }
}
