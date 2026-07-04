using Godot;

public partial class Core : StaticBody2D, IEntity
{

	[Signal] public delegate void HealthChangedEventHandler(int health, int maxhp);
	[Signal] public delegate void DiedEventHandler();

	[Export] public Team Team { get; set; } = Team.Player;

	[Export] public int MaxHp { get; set; } = 10000;

	[ExportGroup("Leash")]
	[Export] public Vector2 LeashOffset { get; set; } = new Vector2(-80f, 0f);  // 타깃 기준 목표 지점 오프셋(왼쪽에 고정)
	[Export] public float BossWaveScale { get; set; } = 0.6f;  // 목줄 걸릴 때(10웨이브) 코어 축소 배율
	[Export] public float LeashCatchUpSpeed { get; set; } = 12f;  // 목표 지점과의 간격을 좁히는 속도(클수록 즉시 스냅에 가까움)

	private bool _leashActive;
	private Node2D _leashTarget;
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

	// 보스 웨이브 진입 시 호출. target(플레이어) 왼쪽에 고정 오프셋으로 딱 붙는 목줄 모드를 켠다.
	// target이 null이면 목줄을 끈다(원래대로 고정).
	public void StartLeash(Node2D target)
	{
		_leashTarget = target;
		_leashActive = target != null;
		Scale = target != null ? new Vector2(BossWaveScale, BossWaveScale) : Vector2.One;
	}

	// 주사위4(속박) 동안 위치 갱신을 멈추고 제자리에 고정한다.
	public void SetRooted(bool rooted)
	{
		_rooted = rooted;
	}

	// 스프링 관성은 없지만, 목표 지점까지 지수 감쇠로 부드럽게 좁혀간다(즉시 스냅 X).
	// 속박이 풀린 직후처럼 간격이 크게 벌어져 있던 경우에도 순간이동 없이 자연스럽게 따라붙는다.
	public override void _PhysicsProcess(double delta)
	{
		if (_rooted || !_leashActive || _leashTarget == null || !IsInstanceValid(_leashTarget))
			return;

		Vector2 targetPosition = _leashTarget.GlobalPosition + LeashOffset;
		float t = 1f - Mathf.Exp(-LeashCatchUpSpeed * (float)delta);
		GlobalPosition = GlobalPosition.Lerp(targetPosition, t);
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
