public static class MagicCombo
{
    public static Elemental PreviousElemental(MagicNode node)
    {
        if (node.AffectedElementals == null || node.AffectedElementals.Count == 0)
            return Elemental.None;

        return node.AffectedElementals[^1];
    }

    public static bool ShouldSplitProjectile(MagicNode node, Elemental current)
        => PreviousElemental(node) == Elemental.Earth && current == Elemental.Ice;

    public static bool ShouldPierce(MagicNode node, Elemental current)
        => node.ForcePierce || (current == Elemental.Wind && PreviousElemental(node) != Elemental.Earth);

    public static HitInfo BuildHit(MagicNode node, Elemental current, Monster target = null)
    {
        Elemental previous = PreviousElemental(node);
        bool suppressWind = node.SuppressWindElementEffects
            && (previous == Elemental.Wind || current == Elemental.Wind);

        if (suppressWind)
            previous = Elemental.None;

        var hit = new HitInfo
        {
            Damage = AdjustedDamage(node.Stat.Damage, previous, current),
            SourceTeam = Team.Player,
            Element = current,
            SuppressElementEffect = suppressWind && current == Elemental.Wind
        };

        ApplyCombinationEffects(ref hit, previous, current, node, target);
        return hit;
    }

    private static int AdjustedDamage(int baseDamage, Elemental previous, Elemental current)
    {
        float multiplier = 1f;

        if (previous == Elemental.Fire && current == Elemental.Ice)
            multiplier = 0.5f;
        else if (previous == Elemental.Fire && (current == Elemental.Earth || current == Elemental.Wind))
            multiplier = 1.25f;
        else if (previous == Elemental.Ice && current == Elemental.Fire)
            multiplier = 0.5f;
        else if (previous == Elemental.Ice && (current == Elemental.Earth || current == Elemental.Wind))
            multiplier = 1.25f;
        else if (previous == Elemental.Earth && (current == Elemental.Fire || current == Elemental.Ice))
            multiplier = 1.25f;
        else if (previous == Elemental.Earth && current == Elemental.Wind)
            multiplier = 0.5f;
        else if (previous == Elemental.Wind && (current == Elemental.Fire || current == Elemental.Ice))
            multiplier = 1.25f;
        else if (previous == Elemental.Wind && current == Elemental.Earth)
            multiplier = 0.5f;

        
        return Mathf.RoundToInt(baseDamage * multiplier);
    }

    private static void ApplyCombinationEffects(
        ref HitInfo hit,
        Elemental previous,
        Elemental current,
        MagicNode node,
        Monster target)
    {
        if (previous == Elemental.Fire && current == Elemental.Ice)
        {
            Vfx.ExplanationBuff.Throw(new VfxExplanationBuffData
            {
                BuffIndex = 1, GlobalPosition = node.GlobalPosition
            });
            hit.SuppressElementEffect = true;
        }
        else if (previous == Elemental.Fire && current == Elemental.Earth)
        {
            Vfx.ExplanationBuff.Throw(new VfxExplanationBuffData
            {
                BuffIndex = 0, GlobalPosition = node.GlobalPosition
            });
            hit.EarthDurationMultiplier = 1.5f;
            hit.ApplyFireEffect = true;
        }
        else if (previous == Elemental.Fire && current == Elemental.Wind)
        {
            Vfx.ExplanationBuff.Throw(new VfxExplanationBuffData
            {
                BuffIndex = 0, GlobalPosition = node.GlobalPosition
            });
            hit.ApplyFireEffect = true;
        }
        else if (previous == Elemental.Ice && current == Elemental.Fire)
        {
            
            Vfx.ExplanationBuff.Throw(new VfxExplanationBuffData
            {
                BuffIndex = 1, GlobalPosition = node.GlobalPosition
            });
            hit.SuppressElementEffect = true;
        }
        else if (previous == Elemental.Ice && current == Elemental.Earth)
        {
            if (target != null && target != node.PrimaryTarget)
            {            
                Vfx.ExplanationBuff.Throw(new VfxExplanationBuffData
                {
                    BuffIndex = 0, GlobalPosition = node.GlobalPosition
                });
                hit.SuppressElementEffect = true;
                hit.ApplyIceEffect = true;
            }
        }
        else if (previous == Elemental.Ice && current == Elemental.Wind)
        {
            Vfx.ExplanationBuff.Throw(new VfxExplanationBuffData
            {
                BuffIndex = 0, GlobalPosition = node.GlobalPosition
            });
            hit.ApplyIceEffect = true;
        }
        else if (previous == Elemental.Earth && current == Elemental.Fire)
        {            
            Vfx.ExplanationBuff.Throw(new VfxExplanationBuffData
            {
                BuffIndex = 0, GlobalPosition = node.GlobalPosition
            });
            hit.ApplyFireEffect = true;
        }
        else if (previous == Elemental.Wind && current == Elemental.Fire)
        {
            Vfx.ExplanationBuff.Throw(new VfxExplanationBuffData
            {
                BuffIndex = 0, GlobalPosition = node.GlobalPosition
            });
            hit.FireDurationMultiplier = 1.5f;
        }
        else if (previous == Elemental.Wind && current == Elemental.Ice)
        {
            Vfx.ExplanationBuff.Throw(new VfxExplanationBuffData
            {
                BuffIndex = 0, GlobalPosition = node.GlobalPosition
            });
            hit.ApplyVulnerableEffect = true;
        }
        else if (previous == Elemental.Wind && current == Elemental.Earth)
        {
            Vfx.ExplanationBuff.Throw(new VfxExplanationBuffData
            {
                BuffIndex = 1, GlobalPosition = node.GlobalPosition
            });
            hit.SuppressElementEffect = true;
        }
    }
}
