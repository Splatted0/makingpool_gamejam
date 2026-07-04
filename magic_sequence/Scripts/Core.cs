using Godot;

public partial class Core : StaticBody2D, IEntity
{

	[Signal] public delegate void HealthChangedEventHandler(int health, int maxhp);
	[Signal] public delegate void DiedEventHandler();

	[Export] public Team Team { get; set; } = Team.Player;

	[Export] public int MaxHp { get; set; } = 10000;

	[ExportGroup("Leash")]
	[Export] public float LeashAccel { get; set; } = 8f;      // 목줄이 당기는 힘(스프링 상수)
	[Export] public float LeashDamping { get; set; } = 0.85f; // 초당 속도 감쇠율(0~1, 클수록 안 줄어듦)
	[Export] public float LeashMaxSpeed { get; set; } = 600f;

	private bool _leashActive;
	private Node2D _leashTarget;
	private Vector2 _leashVelocity;
	private bool _rooted;

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

	// 보스 웨이브 진입 시 호출. target(플레이어)을 관성 붙여 뒤쫓아가는 목줄 모드를 켠다.
	// target이 null이면 목줄을 끈다(원래대로 고정).
	public void StartLeash(Node2D target)
	{
		_leashTarget = target;
		_leashActive = target != null;
		_leashVelocity = Vector2.Zero;
	}

	// 주사위4(속박) 동안 목줄 당김을 무시하고 제자리에 고정한다.
	public void SetRooted(bool rooted)
	{
		_rooted = rooted;
		if (rooted)
			_leashVelocity = Vector2.Zero;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_rooted || !_leashActive || _leashTarget == null || !IsInstanceValid(_leashTarget))
			return;

		Vector2 toTarget = _leashTarget.GlobalPosition - GlobalPosition;
		_leashVelocity += toTarget * LeashAccel * (float)delta;
		_leashVelocity *= Mathf.Pow(LeashDamping, (float)delta);
		_leashVelocity = _leashVelocity.LimitLength(LeashMaxSpeed);

		GlobalPosition += _leashVelocity * (float)delta;
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
