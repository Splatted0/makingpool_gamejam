using System.Collections.Generic;

public partial class Main : Node
{
    [Signal] public delegate void GoldChangedEventHandler(int gold);
    [Signal] public delegate void WaveChangedEventHandler(int wave);
    [Signal] public delegate void WandsChangedEventHandler();

    public int Wave { get; private set; }
    public int Gold { get; private set; }
    
    [ExportCategory("Nodes")]
    [Export] public MainMenu MainMenu;
    [Export] public BattleWorldHud BattleWorldHud;
    [Export] public MagicInfoLayer MagicInfoLayer;
    [Export] public EnhanceManager EnhanceManager;
    [Export] public StateChanger StateChanger;

    [ExportCategory("Resources")]
    [Export] public Wand[] Wands;
    [Export] public MagicPool MagicPool;
    [Export] public WandPool WandPool;
    [Export] public EnhanceData[] EnhanceDataList;
    
    public override void _EnterTree()
    {
        Blackboard.Main = this;

    }

    public override void _Ready()
    {
        StateChanger.Start();
    }

    public void SetGold(int gold)
    {
        int newGold = Math.Max(0, gold);

        if (Gold == newGold)
            return;

        Gold = newGold;
        EmitSignal(SignalName.GoldChanged, Gold);
    }

    public void AddGold(int amount)
    {
        if (amount <= 0)
            return;

        SetGold(Gold + amount);
    }

    public bool TrySpendGold(int amount)
    {
        if (amount <= 0)
            return true;

        if (Gold < amount)
            return false;

        SetGold(Gold - amount);
        return true;
    }

    public void SetWave(int wave)
    {
        Wave = Math.Max(0, wave);
        EmitSignal(SignalName.WaveChanged, Wave);
    }

    public void AddWand(Wand wand)
    {
        var newWands = new Wand[Wands.Length + 1];
        Wands.CopyTo(newWands, 0);
        newWands[Wands.Length] = wand;
        Wands = newWands;
        EmitSignal(SignalName.WandsChanged);
    }
}
