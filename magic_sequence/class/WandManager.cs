public partial class WandManager : Node
{
    [Export] public Player Player { get; set; }
    [Export] public Core Core { get; set; }
    [Export] public Node Projectiles { get; set; }
    [Export] public WandNode WandNode1 { get; set; }
    [Export] public WandNode WandNode2 { get; set; }
    [Export] public WandNode WandNode3 { get; set; }
    [Export] public Arsenal Arsenal { get; set; }

    [Export] public float SpawnDistance { get; set; } = 50.0f;
    [Export] public double FireCooldown { get; set; } = 0.5;

    private readonly double[] _fireCooldownLeft = new double[3];

    public override void _Ready()
    {
        ResolveReferences();
        SetupWands();

        ValidateInputAction("first_wand");
        ValidateInputAction("second_wand");
        ValidateInputAction("third_wand");
    }

    public override void _Process(double delta)
    {
        if (Blackboard.BattleWorldHud != null && !Blackboard.BattleWorldHud.Visible)
            return;

        for (int i = 0; i < _fireCooldownLeft.Length; i++)
            _fireCooldownLeft[i] -= delta;

        if (Input.IsActionJustPressed("first_wand"))
            Fire(0);

        if (Input.IsActionJustPressed("second_wand"))
            Fire(1);

        if (Input.IsActionJustPressed("third_wand"))
            Fire(2);
    }

    public void SetupWands()
    {
        Wand[] wands = Blackboard.Wands;
        WandNode[] nodes = GetWandNodes();

        for (int i = 0; i < nodes.Length; i++)
        {
            Wand wand = wands != null && i < wands.Length ? wands[i] : null;
            nodes[i]?.Setup(wand);
        }

        Arsenal?.Refresh();
    }

    private void Fire(int wandIndex)
    {
        ResolveReferences();

        if (Player == null || Projectiles == null || wandIndex < 0 || wandIndex >= _fireCooldownLeft.Length)
            return;

        if (_fireCooldownLeft[wandIndex] > 0.0)
            return;

        WandNode wandNode = GetWandNodes()[wandIndex];

        if (wandNode == null)
            return;

        Vector2 direction = GetAwayFromCoreDirection();
        Vector2 spawnPosition = Player.GlobalPosition + direction * SpawnDistance;
        bool fired = false;

        foreach (MagicNode magicNode in wandNode.Active())
        {
            Projectiles.AddChild(magicNode);
            magicNode.GlobalPosition = spawnPosition;
            magicNode.Fire(direction);
            magicNode.OnSpawn();
            fired = true;
        }

        if (fired)
            _fireCooldownLeft[wandIndex] = FireCooldown;

        Arsenal?.Refresh();
    }

    public WandNode[] GetWandNodes() => new[] { WandNode1, WandNode2, WandNode3 };

    private void ResolveReferences()
    {
        Player ??= GetNodeOrNull<Player>("../EntityContainer/Player");
        Core ??= GetNodeOrNull<Core>("../EntityContainer/Core");
        Projectiles ??= GetNodeOrNull<Node>("../EntityContainer/Projectiles");
        Arsenal ??= GetNodeOrNull<Arsenal>("../../UIRoot/Arsenal");
    }

    private static void ValidateInputAction(string action)
    {
        if (!InputMap.HasAction(action))
            GD.PrintErr($"[WandManager] Input action '{action}' does not exist. Check Project Settings > Input Map.");
    }

    private Vector2 GetAwayFromCoreDirection()
    {
        if (Core == null || !IsInstanceValid(Core))
            return Vector2.Right;

        Vector2 direction = Player.GlobalPosition - Core.GlobalPosition;

        if (direction.LengthSquared() < 0.001f)
            return Vector2.Right;

        return direction.Normalized();
    }
}
