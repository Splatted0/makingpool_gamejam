using Godot;

public partial class EnhanceManager : CanvasLayer
{
    [Signal] public delegate void EnhanceEndEventHandler();

    [Export] public MagicChangeManager MagicChanceManager;
    [Export] public WandUi GetWandUi;
    [Export] public BaseButton ExitButton;
    [Export] public BaseButton GetWandButton;
    [Export] public Button MagicRerollButton;
    [Export] public Button WandRerollButton;
    [Export] public BaseButton AddHealthButton;
    [Export] public RichTextLabel HealthLabel;
    [Export] public Label GoldLabel;
    [Export] public Wand[] StartWand;

    private Wand _getWand;
    private EnhanceData _enhanceData;
    private int _getMagicCount;
    
    public override void _Ready()
    {
        ExitButton.Pressed += OnExitButtonPressed;
        GetWandButton.Pressed += OnGetWandButtonPressed;
        MagicRerollButton.Pressed += OnMagicRerollButtonPressed;
        WandRerollButton.Pressed += OnWandRerollButtonPressed;
        MagicChanceManager.MagicChangeEnd += OnMagicChangeEnd;
        AddHealthButton.Pressed += OnAddHealthButtonPressed;
        Blackboard.Core.HealthChanged += OnHealthChanged;
        Blackboard.Main.GoldChanged += OnGoldChanged;
        Setup();
    }

    public void Setup()
    {
        ExitButton.Visible = false;
        _getMagicCount = 0;

        int idx = Math.Clamp(Blackboard.Wave, 0, Blackboard.EnhanceDataList.Length - 1);
        _enhanceData = Blackboard.EnhanceDataList[idx];
        
        Magic[] magics = DropUtil.GetMagicDrops(Blackboard.MagicPool, _enhanceData.DropMagicCount, _enhanceData);
        MagicChanceManager.Setup(magics);
        MagicChanceManager.Visible = true;

        MagicRerollButton.Text = "$" + _enhanceData.MagicRerollCost;
        MagicRerollButton.Visible = magics.Length > 0;

        bool showWand = _enhanceData.IsWandDrop && Blackboard.Wands.Length < 3;
        if (showWand)
        {
            Wand[] wands = Blackboard.Wands.Length == 0 ? StartWand : DropUtil.GetWandDrops(Blackboard.WandPool, 1);
            wands[0].Setup();
            _getWand = wands[0].Duplicate() as Wand;
            _getWand?.Setup();
            GetWandUi.Setup(_getWand);
        }
        GetWandUi.Visible = showWand;
        GetWandButton.Visible = showWand;
        WandRerollButton.Text = "$" + _enhanceData.WandRerollCost;
        WandRerollButton.Visible = showWand;
    }

    private void OnMagicChangeEnd()
    {
        if (_getMagicCount >= _enhanceData.MustGetMagicCount) return;

        if (_enhanceData.DropMagicCount - MagicChanceManager.GetMagicRemainCount > 0)
            _getMagicCount++;

        if (_getMagicCount >= _enhanceData.MustGetMagicCount)
        {
            ExitButton.Visible = true;
            MagicRerollButton.Visible = false;
            MagicChanceManager.Visible = false;
        }
    }

    private void OnGetWandButtonPressed()
    {
        Blackboard.Main.AddWand(_getWand);
        GetWandUi.Visible = false;
        GetWandButton.Visible = false;
        WandRerollButton.Visible = false;
    }

    private void OnMagicRerollButtonPressed()
    {
        if (!Blackboard.TrySpendGold(_enhanceData.MagicRerollCost)) return;

        ExitButton.Visible = false;
        Magic[] magics = DropUtil.GetMagicDrops(Blackboard.MagicPool, _enhanceData.DropMagicCount, _enhanceData);
        MagicChanceManager.Setup(magics);
        MagicRerollButton.Visible = magics.Length > 0;
    }

    private void OnWandRerollButtonPressed()
    {
        if (!Blackboard.TrySpendGold(_enhanceData.WandRerollCost)) return;

        Wand[] wands = DropUtil.GetWandDrops(Blackboard.WandPool, 1);
        wands[0].Setup();
        _getWand = wands[0].Duplicate() as Wand;
        _getWand?.Setup();
        GetWandUi.Setup(_getWand);
    }

    private void OnAddHealthButtonPressed()
    {
        if (!Blackboard.TrySpendGold(150)) return;

        Blackboard.Core.Health = Mathf.Min(Blackboard.Health + 1000, Blackboard.Core.MaxHp);
    }

    private void OnHealthChanged(int health, int _maxhp)
    {
        HealthLabel.Text = "현재 체력" +  health+ "/"+ _maxhp;   
    }
    
    private void OnExitButtonPressed()
    {
        Blackboard.MagicInfoLayer.OutPressed();
        EmitSignal(SignalName.EnhanceEnd);
    }

    private void OnGoldChanged(int gold)
    {
        GoldLabel.Text = "보유 금액 : $" + gold.ToString();
    }
}
