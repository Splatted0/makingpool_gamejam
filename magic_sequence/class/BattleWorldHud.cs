using Godot;

public partial class BattleWorldHud : CanvasLayer
{
    [Export] public RoundManager RoundManager { get; private set; }
    [Export] public Node2D EntityContainer { get; private set; }
    [Export] public Core Core { get; private set; }
    [Export] public WandManager WandManager { get; private set; }
    
    [Export] private ProgressBar _coreHpBar;
    [Export] private Label _coreHpText;
    [Export] private Label _goldText;
    [Export] private ProgressBar _roundInfoBar;
    [Export] private Label _roundInfoText;

    private ColorRect _pauseDim;

    private int _lastGold = int.MinValue;

    public override void _Ready()
    {
        ProcessMode = Node.ProcessModeEnum.Always;

        if (Core != null)
        {
            Core.HealthChanged += OnCoreHpChanged;
            OnCoreHpChanged(Core.Health, Core.MaxHp);
        }
        else
        {
            GD.PrintErr("[BattleWorldHud] Core node not found.");
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
        if (_roundInfoText == null && _roundInfoBar == null)
            return;

        RoundManager roundManager = RoundManager ?? Blackboard.RoundManager;
        Spawner spawner = roundManager?.Spawner;
        int roundNumber = roundManager?.RoundNumber ?? Blackboard.Wave;

        Boss boss = FindAliveBoss();
        if (boss?.Data != null)
        {
            int bossMaxHealth = Mathf.Max(boss.Data.MaxHealth, 1);
            int bossHealth = Mathf.Clamp(boss.Health, 0, bossMaxHealth);

            if (_roundInfoBar != null)
            {
                _roundInfoBar.MinValue = 0;
                _roundInfoBar.MaxValue = bossMaxHealth;
                _roundInfoBar.Value = bossHealth;
            }

            if (_roundInfoText != null)
                _roundInfoText.Text = $"Boss: {bossHealth}/{bossMaxHealth}";

            return;
        }

        int total = Mathf.Max(spawner?.TotalSpawnCount ?? 0, 0);
        int remaining = Mathf.Clamp(spawner?.RemainingMonsterCount ?? 0, 0, total);

        if (_roundInfoBar != null)
        {
            _roundInfoBar.MinValue = 0;
            _roundInfoBar.MaxValue = Mathf.Max(total, 1);
            _roundInfoBar.Value = remaining;
        }

        if (_roundInfoText != null)
            _roundInfoText.Text = $"Round {roundNumber}: {remaining}/{total}";
    }

    private Boss FindAliveBoss()
    {
        if (EntityContainer == null)
            return null;

        foreach (Node child in EntityContainer.GetChildren())
        {
            if (child is Boss boss && IsInstanceValid(boss) && boss.Health > 0)
                return boss;
        }

        return null;
    }
}
