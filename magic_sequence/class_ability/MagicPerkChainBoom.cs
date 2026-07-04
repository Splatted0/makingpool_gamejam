using System.Collections.Generic;

[GlobalClass]
public partial class MagicPerkChainBoom : MagicPerk
{
    [Export] public int TargetCount { get; private set; } = 3;
    [Export] public int EnhancedTargetCount { get; private set; } = 5;
    [Export] public float DamageMultiplier { get; private set; } = 1f;
    [Export] public float EnhancedDamageMultiplier { get; private set; } = 1f;

    public override void OnNodeArrival(MagicNode node, List<Monster> targets, int progressedFrame)
    {
        if (node?.MagicSpell == null || progressedFrame != 0)
            return;

        List<Monster> candidates = GetAliveMonsters();
        Shuffle(candidates);

        int hits = Mathf.Min(CurrentTargetCount, candidates.Count);
        for (int i = 0; i < hits; i++)
        {
            Monster target = candidates[i];
            HitInfo hit = MagicCombo.BuildHit(node, node.MagicSpell.Elemental, target);
            hit.Damage = Mathf.Max(0, Mathf.RoundToInt(hit.Damage * CurrentDamageMultiplier));
            target.Hit(hit);
        }
    }

    public override void MagicEnhance()
    {
        IsEnhanced = true;
    }

    private int CurrentTargetCount => IsEnhanced ? EnhancedTargetCount : TargetCount;
    private float CurrentDamageMultiplier => IsEnhanced ? EnhancedDamageMultiplier : DamageMultiplier;

    private static List<Monster> GetAliveMonsters()
    {
        var monsters = new List<Monster>();
        Node2D container = Blackboard.EntityContainer;
        if (container == null)
            return monsters;

        foreach (Node child in container.GetChildren())
        {
            if (child is Monster monster && monster.Health > 0)
                monsters.Add(monster);
        }

        return monsters;
    }

    private static void Shuffle(List<Monster> monsters)
    {
        for (int i = monsters.Count - 1; i > 0; i--)
        {
            int j = (int)(GD.Randi() % (uint)(i + 1));
            (monsters[i], monsters[j]) = (monsters[j], monsters[i]);
        }
    }
}
