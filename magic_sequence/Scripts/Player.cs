using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export] public float Speed { get; set; } = 300.0f;

	[Export] public PackedScene MagicMissileScene { get; set; }
	[Export] public float MissileSpawnDistance { get; set; } = 45.0f;
	[Export] public double MissileCooldown { get; set; } = 0.5;

	private Core _core;
	private double _missileCooldownLeft = 0.0;

	public override void _Ready()
	{
		AddToGroup("player");
		_core = GetTree().GetFirstNodeInGroup("core") as Core;
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;


		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 direction = Input.GetVector("move_left", "move_right", "move_up", "move_down");
		if (direction != Vector2.Zero)
		{
			velocity = direction * Speed;
		}
		else
		{
			velocity = velocity.MoveToward(Vector2.Zero, Speed);
		}

		Velocity = velocity;
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
			GD.PrintErr("MagicMissileScene is not assigned.");
			return;
		}

		if (_core == null || !IsInstanceValid(_core))
		{
			_core = GetTree().GetFirstNodeInGroup("core") as Core;
		}

		Vector2 direction = Vector2.Right;

		if (_core != null)
		{
			direction = (GlobalPosition - _core.GlobalPosition).Normalized();

			if (direction == Vector2.Zero)
				direction = Vector2.Right;
		}

		MagicMissile missile = MagicMissileScene.Instantiate<MagicMissile>();

		missile.GlobalPosition = GlobalPosition + direction * MissileSpawnDistance;
		missile.Direction = direction;
		missile.Rotation = direction.Angle();

		Node projectileParent = GetTree().CurrentScene.GetNodeOrNull<Node>("Projectiles");

		if (projectileParent != null)
			projectileParent.AddChild(missile);
		else
			GetTree().CurrentScene.AddChild(missile);
	}
}
