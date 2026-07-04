
using System.Collections.Generic;

public static class Blackboard
{
    public static Main Main { get; set; }
    public static int Wave => Main.Wave;
    public static int Gold => Main.Gold;
    
    public static MainMenu MainMenu => Main.MainMenu;

    public static Tutorial Tutorial => Main.Tutorial;
    public static EnhanceManager EnhanceManager => Main.EnhanceManager;
    public static MagicInfoLayer MagicInfoLayer => Main.MagicInfoLayer;
    public static Wand[] Wands => Main.Wands;
    public static EnhanceData[] EnhanceDataList => Main.EnhanceDataList;
    
    public static MagicPool MagicPool => Main.MagicPool;
    public static WandPool WandPool => Main.WandPool;

    public static BattleWorldHud BattleWorldHud => Main.BattleWorldHud;
        public static RoundManager RoundManager => BattleWorldHud.RoundManager;
        public static Node2D EntityContainer => BattleWorldHud.EntityContainer;
        public static Core Core => BattleWorldHud.Core;
        public static int Health => BattleWorldHud.Core.Health;
        public static WandManager WandManager => BattleWorldHud.WandManager;
    

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