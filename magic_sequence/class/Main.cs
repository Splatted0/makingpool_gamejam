using System.Collections.Generic;

public partial class Main : Node
{
    
    [Signal] public delegate void HealthChangedEventHandler(int health, int maxHealth);
    [Signal] public delegate void GoldChangedEventHandler(int gold);
    [Signal] public delegate void WaveChangedEventHandler(int wave);

    public int Wave { get; private set; }
    public int Health { get; private set; }
    public int MaxHealth { get; private set; } = 1;
    public int Gold { get; private set; }
    
    [ExportCategory("Nodes")]
    [Export] public MainMenu MainMenu;
    [Export] public CanvasLayer BattleWorld;
    [Export] public MagicInfoLayer MagicInfoLayer;
    [Export] public EnhanceManager EnhanceManager;
    [Export] public StateChanger StateChanger;

    [ExportCategory("Resources")]
    [Export] public Wand[] Wands;
    [Export] public MagicPool MagicPool;
    [Export] public WandPool WandPool;

    private RoundManager _roundManager;
    private WandManager _wandManager;
    
    
    public override void _EnterTree()
    {
        Blackboard.Main = this;

    }

    public override void _Ready()
    {
        ResolveBattleReferences();
        ConnectRoundFlow();
        StateChanger.Start();
    }

    private void ResolveBattleReferences()
    {
        _roundManager = BattleWorld?.GetNodeOrNull<RoundManager>("BattleCenter/RoundManager");
        _wandManager = BattleWorld?.GetNodeOrNull<WandManager>("BattleCenter/WandManager");
    }

    private void ConnectRoundFlow()
    {
        if (_roundManager != null)
            _roundManager.RoundEnded += OnRoundEnded;

        if (EnhanceManager != null)
            EnhanceManager.InhanceFinished += OnInhanceFinished;

        if (MainMenu != null)
            MainMenu.GameStartPressed += OnGameStartPressed;
    }

    private async void OnGameStartPressed()
    {
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        _wandManager?.SetupWands();
        _roundManager?.StartRound();
    }

    private void OnRoundEnded()
    {
        BattleWorld.Visible = false;
        EnhanceManager.Visible = true;
        EnhanceManager.Setup();
    }

    private void OnInhanceFinished()
    {
        EnhanceManager.Visible = false;
        BattleWorld.Visible = true;
        _wandManager?.SetupWands();
        _roundManager?.StartRound();
    }
    
    
    public void SetHealth(int health, int maxHealth)
    {
        MaxHealth = Math.Max(1, maxHealth);
        Health = Math.Clamp(health, 0, MaxHealth);

        EmitSignal(SignalName.HealthChanged, Health, MaxHealth);
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
    }
}
