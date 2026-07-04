using Godot;

public partial class BattleWorldHud : CanvasLayer
{
    [Export] public RoundManager RoundManager { get; private set; }
    [Export] public Node2D EntityContainer { get; private set; }

    public Core Core { get; private set; }

    private Control _uiRoot;
    private ProgressBar _coreHpBar;
    private Label _coreHpText;
    private Label _goldText;
    private Label _roundInfoText;
    private Button _pauseButton;

    private ColorRect _pauseDim;

    private int _lastGold = int.MinValue;

    public override void _Ready()
    {
        ProcessMode = Node.ProcessModeEnum.Always;

        _uiRoot = GetNodeOrNull<Control>("UIRoot");

        Core = GetNodeOrNull<Core>("BattleCenter/Core")
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

        _roundInfoText = GetNodeOrNull<Label>("UIRoot/Roundinfo/RoundInfo");
        _pauseButton = GetNodeOrNull<Button>("UIRoot/PauseButton");

        CreateOverlayNodes();

        if (Core != null)
        {
            Core.HealthChanged += OnCoreHpChanged;
            OnCoreHpChanged(Core.Health, Core.MaxHp);
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
        UpdateRoundInfoText();
    }

    public override void _Process(double delta)
    {
        if (_lastGold != Blackboard.Gold)
            UpdateGoldText(Blackboard.Gold);

        UpdateRoundInfoText();
    }

    public override void _ExitTree()
    {
        if (Core != null)
            Core.HealthChanged -= OnCoreHpChanged;

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

        _pauseButton?.MoveToFront();
    }

    private void OnCoreHpChanged(int health, int maxHp)
    {
        if (_coreHpBar != null)
        {
            _coreHpBar.MinValue = 0;
            _coreHpBar.MaxValue = maxHp;
            _coreHpBar.Value = health;
        }

        if (_coreHpText != null)
        {
            int percent = maxHp <= 0 ? 0 : Mathf.RoundToInt(health * 100f / maxHp);
            double healthk = health/1000;
            double maxHPk = maxHp/1000;
            _coreHpText.Text = $"{System.Math.Truncate(healthk)}k/{System.Math.Truncate(maxHPk)}k ({percent}%)";
        }
    }

    private void UpdateGoldText(int gold)
    {
        _lastGold = gold;

        if (_goldText != null)
            _goldText.Text = $"Gold: ${gold}";
    }

    private void UpdateRoundInfoText()
    {
        if (_roundInfoText == null)
            return;

        RoundManager roundManager = RoundManager ?? Blackboard.RoundManager;
        Spawner spawner = roundManager?.Spawner;
        int roundNumber = roundManager?.RoundNumber ?? Blackboard.Wave;

        int total = spawner?.TotalSpawnCount ?? 0;
        int remaining = spawner?.RemainingMonsterCount ?? 0;

        _roundInfoText.Text = $"Round {roundNumber}: {remaining}/{total}";
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

}
