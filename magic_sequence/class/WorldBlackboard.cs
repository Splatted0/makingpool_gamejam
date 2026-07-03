
using System.Collections.Generic;

public static class Blackboard
{
    public static Main Main { private get; set; }
    public static int Health => Main.Health;
    public static int Wave => Main.Wave;
    public static int Gold => Main.Gold;
    public static Node BattleWorld => Main.BattleWorld;
    public static Wand[] Wands => Main.Wands;

    public static void SetHealth(int health, int maxHealth)
    {
        Main?.SetHealth(health, maxHealth);
    }

    public static void SetGold(int gold)
    {
        Main?.SetGold(gold);
    }

    public static void AddGold(int amount)
    {
        Main?.AddGold(amount);
    }

    public static bool TrySpendGold(int amount)
    {
        return Main != null && Main.TrySpendGold(amount);
    }

    public static void SetWave(int wave)
    {
        Main?.SetWave(wave);
    }
}