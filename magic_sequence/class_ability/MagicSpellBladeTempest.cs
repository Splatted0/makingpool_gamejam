using System.Collections.Generic;

[GlobalClass]
public partial class MagicSpellBladeTempest : MagicSpell
{
    [Export] public float VulnerableDamageMultiplier { get; private set; } = 1.1f;
    [Export] public float EnhancedVulnerableDamageMultiplier { get; private set; } = 1.2f;
    [Export] public float VulnerableDuration { get; private set; } = 2f;

    public override void SpawnEffect(MagicNode node)
    {
    }

    public override void MoveEffect(MagicNode node, List<Monster> targets, float fdelta)
    {
        foreach (Monster monster in targets)
        {
            if (node.TryMarkMoveHit(monster))
                monster.Hit(BuildHit(node, monster));
        }
    }

    public override void ArrivalEffect(MagicNode node, List<Monster> targets, int progressedFrame)
    {
    }

    public override void MagicEnhance()
    {
        IsEnhanced = true;
    }

    private HitInfo BuildHit(MagicNode node, Monster target)
    {
        HitInfo hit = MagicCombo.BuildHit(node, Elemental.Wind, target);
        hit.ApplyVulnerableEffect = true;
        hit.VulnerableDamageMultiplier = IsEnhanced ? EnhancedVulnerableDamageMultiplier : VulnerableDamageMultiplier;
        hit.VulnerableDurationSeconds = VulnerableDuration;
        return hit;
    }
}
