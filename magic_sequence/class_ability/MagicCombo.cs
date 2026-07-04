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
        => current == Elemental.Wind && PreviousElemental(node) != Elemental.Earth;

    public static HitInfo BuildHit(MagicNode node, Elemental current, Monster target = null)
    {
        Elemental previous = PreviousElemental(node);
        var hit = new HitInfo
        {
            Damage = AdjustedDamage(node.Stat.Damage, previous, current),
            SourceTeam = Team.Player,
            Element = current
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
            hit.SuppressElementEffect = true;
        }
        else if (previous == Elemental.Fire && current == Elemental.Earth)
        {
            hit.EarthDurationMultiplier = 1.5f;
            hit.ApplyFireEffect = true;
        }
        else if (previous == Elemental.Fire && current == Elemental.Wind)
        {
            hit.ApplyFireEffect = true;
        }
        else if (previous == Elemental.Ice && current == Elemental.Fire)
        {
            hit.SuppressElementEffect = true;
        }
        else if (previous == Elemental.Ice && current == Elemental.Earth)
        {
            if (target != null && target != node.PrimaryTarget)
            {
                hit.SuppressElementEffect = true;
                hit.ApplyIceEffect = true;
            }
        }
        else if (previous == Elemental.Ice && current == Elemental.Wind)
        {
            hit.ApplyIceEffect = true;
        }
        else if (previous == Elemental.Earth && current == Elemental.Fire)
        {
            hit.ApplyFireEffect = true;
        }
        else if (previous == Elemental.Wind && current == Elemental.Fire)
        {
            hit.FireDurationMultiplier = 1.5f;
        }
        else if (previous == Elemental.Wind && current == Elemental.Ice)
        {
            hit.ApplyVulnerableEffect = true;
        }
        else if (previous == Elemental.Wind && current == Elemental.Earth)
        {
            hit.SuppressElementEffect = true;
        }
    }
}
