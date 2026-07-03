
public static class Blackboard
{
    public static Main Main { private get; set; }
    public static int Health => Main.Health;
    public static int Wave => Main.Wave;
    public static int Gold => Main.Gold;
    public static Node BattleWorld => Main.BattleWorld;
}