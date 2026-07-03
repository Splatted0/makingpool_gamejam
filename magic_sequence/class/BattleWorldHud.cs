using Godot;

public partial class BattleWorldHud : CanvasLayer
{
    private Core _core;
    private ProgressBar _coreHpBar;
    private Label _coreHpText;
    private Label _goldText;
    private Button _pauseButton;

    private int _lastGold = int.MinValue;

    public override void _Ready()
    {
        // pause 상태에서도 PauseButton이 눌려야 하므로 UI layer는 항상 processing.
        ProcessMode = Node.ProcessModeEnum.Always;

        _core = GetNodeOrNull<Core>("../BattleCenter/EntityContainer/Core");

        _coreHpBar = GetNodeOrNull<ProgressBar>(
            "UIRoot/CoreStatus/VBoxContainer/CoreHpArea/CoreHpBar"
        );

        _coreHpText = GetNodeOrNull<Label>(
            "UIRoot/CoreStatus/VBoxContainer/CoreHpArea/CoreHpText"
        );

        _goldText = GetNodeOrNull<Label>(
            "UIRoot/CoreStatus/VBoxContainer/GoldPanel/GoldText"
        );

        _pauseButton = GetNodeOrNull<Button>("UIRoot/PauseButton");

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
            UpdatePauseButtonText();
        }
        else
        {
            GD.PrintErr("[BattleWorldHud] PauseButton node not found.");
        }

        UpdateGoldText(Blackboard.Gold);
    }

    public override void _Process(double delta)
    {
        // GoldChanged signal까지 연결하지 않고, Blackboard 값만 감시.
        // 지금 구조에서는 이게 제일 덜 건드리는 방식임.
        if (_lastGold != Blackboard.Gold)
            UpdateGoldText(Blackboard.Gold);
    }

    public override void _ExitTree()
    {
        if (_core != null)
            _core.HpChanged -= OnCoreHpChanged;

        if (_pauseButton != null)
            _pauseButton.Pressed -= OnPauseButtonPressed;
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

    private void OnPauseButtonPressed()
    {
        GetTree().Paused = !GetTree().Paused;
        UpdatePauseButtonText();
    }

    private void UpdatePauseButtonText()
    {
        if (_pauseButton == null)
            return;

        _pauseButton.Text = GetTree().Paused ? "▶" : "||";
    }
}