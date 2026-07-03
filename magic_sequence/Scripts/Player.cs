using Godot;

public partial class Player : CharacterBody2D
{
    [Export] public float Speed { get; set; } = 300.0f;

    [Export] public PackedScene MagicMissileScene { get; set; }
    [Export] public float MissileSpawnDistance { get; set; } = 50.0f;
    [Export] public double MissileCooldown { get; set; } = 0.5;

    private Core _core;
    private double _missileCooldownLeft = 0.0;

    public override void _Ready()
    {
        AddToGroup("player");

        _core = GetTree().GetFirstNodeInGroup("core") as Core;

        if (MagicMissileScene == null)
        {
            MagicMissileScene = GD.Load<PackedScene>("res://Scenes/magic_missile.tscn");
            GD.Print("[Player] MagicMissileScene was null, loaded fallback Scenes/magic_missile.tscn");
        }

        if (!InputMap.HasAction("fire"))
        {
            GD.PrintErr("[Player] Input action 'fire' does not exist. Check Project Settings > Input Map.");
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 direction = Input.GetVector("move_left", "move_right", "move_up", "move_down");

        if (direction != Vector2.Zero)
            Velocity = direction * Speed;
        else
            Velocity = Velocity.MoveToward(Vector2.Zero, Speed);

        MoveAndSlide();
    }

    public override void _Process(double delta)
    {
        _missileCooldownLeft -= delta;

        if (Input.IsActionPressed("fire") && _missileCooldownLeft <= 0.0)
        {
            SpawnMagicMissile();
            _missileCooldownLeft = MissileCooldown;
        }
    }

    private void SpawnMagicMissile()
    {
        if (MagicMissileScene == null)
        {
            GD.PrintErr("[Player] MagicMissileScene is not assigned and fallback load failed.");
            return;
        }

        if (_core == null || !IsInstanceValid(_core))
            _core = GetTree().GetFirstNodeInGroup("core") as Core;

        Vector2 direction = GetAwayFromCoreDirection();
        Vector2 spawnPosition = GlobalPosition + direction * MissileSpawnDistance;

        MagicMissile missile = MagicMissileScene.Instantiate<MagicMissile>();

        missile.Direction = direction;
        missile.Rotation = direction.Angle();

        Node projectileParent = GetNodeOrNull<Node>("../Projectiles");

        if (projectileParent == null)
        {
            GD.PrintErr("[Player] ../Projectiles not found. Missile will be added to player's parent.");
            projectileParent = GetParent();
        }

        projectileParent.AddChild(missile);
        missile.GlobalPosition = spawnPosition;

        GD.Print($"[Player] Spawn missile at {spawnPosition}, direction {direction}");
    }

    private Vector2 GetAwayFromCoreDirection()
    {
        if (_core == null || !IsInstanceValid(_core))
            return Vector2.Right;

        Vector2 direction = GlobalPosition - _core.GlobalPosition;

        if (direction.LengthSquared() < 0.001f)
            return Vector2.Right;

        return direction.Normalized();
    }
}