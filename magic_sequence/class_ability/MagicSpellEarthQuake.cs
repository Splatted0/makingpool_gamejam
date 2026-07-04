using System.Collections.Generic;

[GlobalClass]
public partial class MagicSpellEarthQuake : MagicSpell
{
    private const int StunTickIntervalFrame = 60;

    [Export] public float LingerStunDuration { get; private set; } = 0.5f;
    [Export] public float EnhancedLingerStunDuration { get; private set; } = 0.7f;

    public override void SpawnEffect(MagicNode node)
    {
    }

    public override void MoveEffect(MagicNode node, List<Monster> targets, float fdelta)
    {
        if (targets.Count == 0)
            return;

        node.PrimaryTarget ??= targets[0];
        node.TriggerArrival();
    }

    public override void ArrivalEffect(MagicNode node, List<Monster> targets, int progressedFrame)
    {
        if (progressedFrame == 0)
        {
            foreach (Monster monster in targets)
                monster.Hit(MagicCombo.BuildHit(node, Elemental.Earth, monster));
            return;
        }

        if (progressedFrame % StunTickIntervalFrame != 0)
            return;

        foreach (Monster monster in targets)
        {
            monster.Hit(new HitInfo
            {
                Damage = 0,
                SourceTeam = Team.Player,
                Element = Elemental.None,
                ApplyEarthEffect = true,
                EarthDurationSeconds = IsEnhanced ? EnhancedLingerStunDuration : LingerStunDuration
            });
        }
    }

    public override void MagicEnhance()
    {
        IsEnhanced = true;
    }
}
