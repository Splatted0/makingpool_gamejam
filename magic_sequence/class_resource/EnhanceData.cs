[GlobalClass]
public partial class EnhanceData : Resource
{
    [Export] public int DropMagicCount { get; set; } = 3;
    [Export] public int MustGetMagicCount { get; set; } = 1;
    [Export] public bool IsWandDrop { get; set; } = false;
    [Export] public int WandRerollCost { get; set; } = 50;
    [Export] public int MagicRerollCost { get; set; } = 50;
    [Export] public float Tier1Weight { get; set; } = 1;
    [Export] public float Tier2Weight { get; set; } = 0;
    [Export] public float Tier3Weight { get; set; } = 0;
}
