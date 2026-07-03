public partial class WandManager : Node
{
    [Export] public Player Player { get; set; }
    [Export] public Core Core { get; set; }
    [Export] public Node Projectiles { get; set; }
    [Export] public WandNode WandNode1 { get; set; }
    [Export] public WandNode WandNode2 { get; set; }
    [Export] public WandNode WandNode3 { get; set; }

    [Export] public float SpawnDistance { get; set; } = 50.0f;
    [Export] public double FireCooldown { get; set; } = 0.5;

    private double _fireCooldownLeft;

    public override void _Ready()
    {
        ResolveReferences();
        SetupWands();

        if (!InputMap.HasAction("fire"))
            GD.PrintErr("[WandManager] Input action 'fire' does not exist. Check Project Settings > Input Map.");
    }

    public override void _Process(double delta)
    {
        if (Blackboard.BattleWorldHud != null && !Blackboard.BattleWorldHud.Visible)
            return;

        _fireCooldownLeft -= delta;

        if (Input.IsActionPressed("fire") && _fireCooldownLeft <= 0.0)
        {
            Fire();
            _fireCooldownLeft = FireCooldown;
        }
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
    }

    private void Fire()
    {
        ResolveReferences();

        if (Player == null || Projectiles == null)
            return;

        Vector2 direction = GetAwayFromCoreDirection();
        Vector2 spawnPosition = Player.GlobalPosition + direction * SpawnDistance;

        foreach (WandNode wandNode in GetWandNodes())
        {
            if (wandNode == null)
                continue;

            foreach (MagicNode magicNode in wandNode.Active())
            {
                Projectiles.AddChild(magicNode);
                magicNode.GlobalPosition = spawnPosition;
                magicNode.Fire(direction);
                magicNode.OnSpawn();
            }
        }
    }

    private WandNode[] GetWandNodes() => new[] { WandNode1, WandNode2, WandNode3 };

    private void ResolveReferences()
    {
        Player ??= GetNodeOrNull<Player>("../EntityContainer/Player");
        Core ??= GetNodeOrNull<Core>("../EntityContainer/Core");
        Projectiles ??= GetNodeOrNull<Node>("../EntityContainer/Projectiles");
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
