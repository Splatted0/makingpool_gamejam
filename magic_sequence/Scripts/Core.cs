using Godot;

public partial class Core : StaticBody2D, IEntity
{

	[Signal] public delegate void HealthChangedEventHandler(int health, int maxhp);
	[Signal] public delegate void DiedEventHandler();

	[Export] public Team Team { get; set; } = Team.Player;

	[Export] public int MaxHp { get; set; } = 10000;

	private int _health;
	public int Health
	{
		get => _health;
		set { _health = value; OnHealthChanged(); }
	}

	public override void _Ready()
	{
		Health = MaxHp;
		AddToGroup("core");

	}

	public void Hit(HitInfo hitInfo)
	{
		if (hitInfo.SourceTeam == Team)
			return;

		TakeDamage(hitInfo.Damage);
	}

	public void TakeDamage(int damage)
	{
		if (damage <= 0)
			return;

		Health = Mathf.Max(Health - damage, 0);

		GD.Print($"Core HP: {Health}/{MaxHp}");

		if (Health <= 0)
		{
			EmitSignal(SignalName.Died);
			GD.Print("Core destroyed");
		}
	}

	private void OnHealthChanged()
	{
		EmitSignalHealthChanged(Health,  MaxHp);
	}
}
