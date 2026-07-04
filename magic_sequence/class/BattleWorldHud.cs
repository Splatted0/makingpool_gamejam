using Godot;
using System.Threading.Tasks;

public partial class BattleWorldHud : CanvasLayer
{
    [Export] public RoundManager RoundManager { get; private set; }
    [Export] public Node2D EntityContainer { get; private set; } 
    
    private Core _core;

    private Control _uiRoot;
    private ProgressBar _coreHpBar;
    private Label _coreHpText;
    private Label _goldText;
    private Label _roundStatusText;
    private Button _pauseButton;

    private ColorRect _pauseDim;
    private Label _roundLabel;

    private int _lastGold = int.MinValue;

    public override void _Ready()
    {
        ProcessMode = Node.ProcessModeEnum.Always;

        _uiRoot = GetNodeOrNull<Control>("UIRoot");

        _core = GetNodeOrNull<Core>("BattleCenter/Core")
            ?? GetTree().GetFirstNodeInGroup("core") as Core;

        _coreHpBar = GetNodeOrNull<ProgressBar>(
            "UIRoot/CoreStatus/VBoxContainer/CoreHpArea/CoreHpBar"
        );

        _coreHpText = GetNodeOrNull<Label>(
            "UIRoot/CoreStatus/VBoxContainer/CoreHpArea/CoreHpText"
        );

        _goldText = GetNodeOrNull<Label>(
            "UIRoot/CoreStatus/VBoxContainer/GoldPanel/GoldText"
        );

        _roundStatusText = GetNodeOrNull<Label>("UIRoot/RoundStatusText");
        _pauseButton = GetNodeOrNull<Button>("UIRoot/PauseButton");

        CreateOverlayNodes();

        if (_core != null)
        {
            _core.HpChanged += OnCoreHpChanged;
            OnCoreHpChanged(_core.Health, _core.MaxHp);
        }
        else
        {
            GD.PrintErr("[BattleWorldHud] Core node not found.");
        }

        if (_pauseButton != null)
        {
            _pauseButton.Pressed += OnPauseButtonPressed;
            UpdatePauseVisual();
        }
        else
        {
            GD.PrintErr("[BattleWorldHud] PauseButton node not found.");
        }

        UpdateGoldText(Blackboard.Gold);
    }

    public override void _Process(double delta)
    {
        if (_lastGold != Blackboard.Gold)
            UpdateGoldText(Blackboard.Gold);

        UpdateRoundStatusText();
    }

    public override void _ExitTree()
    {
        if (_core != null)
            _core.HpChanged -= OnCoreHpChanged;

        if (_pauseButton != null)
            _pauseButton.Pressed -= OnPauseButtonPressed;
    }

    private void CreateOverlayNodes()
    {
        if (_uiRoot == null)
        {
            GD.PrintErr("[BattleWorldHud] UIRoot not found.");
            return;
        }

        _pauseDim = new ColorRect();
        _pauseDim.Name = "PauseDim";
        _pauseDim.Color = new Color(0f, 0f, 0f, 0.75f);
        _pauseDim.Visible = false;
        _pauseDim.MouseFilter = Control.MouseFilterEnum.Ignore;
        _pauseDim.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _uiRoot.AddChild(_pauseDim);

        _roundLabel = new Label();
        _roundLabel.Name = "RoundLabel";
        _roundLabel.Text = "";
        _roundLabel.Visible = false;
        _roundLabel.MouseFilter = Control.MouseFilterEnum.Ignore;
        _roundLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _roundLabel.VerticalAlignment = VerticalAlignment.Center;
        _roundLabel.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _roundLabel.AddThemeFontSizeOverride("font_size", 96);
        _uiRoot.AddChild(_roundLabel);

        _roundStatusText ??= new Label();
        _roundStatusText.Name = "RoundStatusText";
        _roundStatusText.Text = "Round - | Monsters -/-";
        _roundStatusText.MouseFilter = Control.MouseFilterEnum.Ignore;
        _roundStatusText.HorizontalAlignment = HorizontalAlignment.Center;
        _roundStatusText.SetAnchorsPreset(Control.LayoutPreset.TopWide);
        _roundStatusText.OffsetTop = 8;
        _roundStatusText.OffsetBottom = 36;
        _roundStatusText.AddThemeFontSizeOverride("font_size", 24);
        _uiRoot.AddChild(_roundStatusText);

        _pauseButton?.MoveToFront();
    }

    private void OnCoreHpChanged(int health, int maxHp)
    {
        Blackboard.SetHealth(health, maxHp);

        if (_coreHpBar != null)
        {
            _coreHpBar.MinValue = 0;
            _coreHpBar.MaxValue = maxHp;
            _coreHpBar.Value = health;
        }

        if (_coreHpText != null)
        {
            int percent = maxHp <= 0 ? 0 : Mathf.RoundToInt(health * 100f / maxHp);
            _coreHpText.Text = $"{health}/{maxHp} ({percent}%)";
        }
    }

    private void UpdateGoldText(int gold)
    {
        _lastGold = gold;

        if (_goldText != null)
            _goldText.Text = $"Gold: {gold}";
    }

    private void UpdateRoundStatusText()
    {
        if (_roundStatusText == null)
            return;

        RoundManager roundManager = RoundManager ?? Blackboard.RoundManager;
        Spawner spawner = roundManager?.Spawner;
        int roundNumber = roundManager?.RoundNumber ?? Blackboard.Wave;

        int total = spawner?.TotalSpawnCount ?? 0;
        int remaining = spawner?.RemainingMonsterCount ?? 0;

        _roundStatusText.Text = $"Round {roundNumber} | Monsters {remaining}/{total}";
    }

    private void OnPauseButtonPressed()
    {
        GetTree().Paused = !GetTree().Paused;
        UpdatePauseVisual();
    }

    private void UpdatePauseVisual()
    {
        bool paused = GetTree().Paused;

        if (_pauseDim != null)
            _pauseDim.Visible = paused;

        if (_pauseButton != null)
        {
            _pauseButton.Text = paused ? "▶" : "||";
            _pauseButton.MoveToFront();
        }
    }

    public async Task ShowRoundIntro(int roundNumber, double seconds)
    {
        if (_roundLabel == null)
            return;

        _roundLabel.Text = $"Round {roundNumber}";
        _roundLabel.Visible = true;
        _roundLabel.MoveToFront();

        await ToSignal(GetTree().CreateTimer(seconds), SceneTreeTimer.SignalName.Timeout);

        if (IsInstanceValid(_roundLabel))
            _roundLabel.Visible = false;
    }
}
