using Godot;

public partial class Core : StaticBody2D, IEntity
{

	[Signal] public delegate void HealthChangedEventHandler(int health, int maxhp);
	[Signal] public delegate void DiedEventHandler();

	[Export] public Team Team { get; set; } = Team.Player;

	[Export] public int MaxHp { get; set; } = 10000;

	[Export] private AnimatedSprite2D _sprite;   // 평상시 "default" 루프, 피격 시 "hit" 단발 재생

	[ExportGroup("Leash")]
	[Export] public Vector2 LeashOffset { get; set; } = new Vector2(-80f, 0f);  // 타깃 기준 목표 지점 오프셋(왼쪽에 고정)
	[Export] public float BossWaveScale { get; set; } = 0.6f;  // 목줄 걸릴 때(10웨이브) 코어 축소 배율
	[Export] public float LeashCatchUpSpeed { get; set; } = 12f;  // 목표 지점과의 간격을 좁히는 속도(클수록 즉시 스냅에 가까움)
	[Export] public float RootShakeMagnitude { get; set; } = 8f;  // 속박(SetRooted) 중 제자리에서 흔들리는 크기(px, 0이면 안 흔들림)
	[Export] public Color RootTintColor { get; set; } = new Color(1.6f, 0.3f, 2.2f, 1f);  // 속박 중 깜빡이는 색(보라)
	[Export] public float RootTintBlinkSpeed { get; set; } = 8f;  // 깜빡이는 속도(클수록 빠르게 명멸)

	[ExportGroup("Glow")]
	[Export] public Color GlowColor { get; set; } = new Color(1.8f, 2.0f, 2.6f, 1f);  // 평상시 발광 색(HDR 느낌으로 채널 1 넘김, 청백)
	[Export] public float GlowSpeed { get; set; } = 3f;  // 발광 명멸 속도

	private bool _leashActive;
	private Node2D _leashTarget;
	private bool _rooted;
	private Vector2 _rootBasePosition;   // 속박 시작 시점 위치(흔들림은 이 지점 기준으로만 흔들고 드리프트하지 않음)
	private float _tintBlinkElapsed;
	private float _glowElapsed;

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

		if (_sprite != null)
			_sprite.AnimationFinished += OnAnimationFinished;
	}

	// 단발 "hit"가 끝나면 기본 애니(default)로 복귀한다("default"는 루프라 이 신호를 안 낸다).
	private void OnAnimationFinished()
	{
		if (_sprite != null && _sprite.Animation == "hit")
			_sprite.Play("default");
	}

	// 보스 웨이브 진입 시 호출. target(플레이어) 왼쪽에 고정 오프셋으로 딱 붙는 목줄 모드를 켠다.
	// target이 null이면 목줄을 끈다(원래대로 고정).
	public void StartLeash(Node2D target)
	{
		_leashTarget = target;
		_leashActive = target != null;
		Scale = target != null ? new Vector2(BossWaveScale, BossWaveScale) : Vector2.One;
	}

	// 주사위4(속박) 동안 목줄 추적을 멈추고 제자리에서 흔들리기만 한다. 보라색 명멸 틴트도 같이 켜고 끈다.
	public void SetRooted(bool rooted)
	{
		if (_rooted == rooted)
			return;   // 속박된 적 없는데 해제가 불리면 _rootBasePosition(0,0)으로 순간이동하는 것 방지

		if (rooted && !_rooted)
		{
			_rootBasePosition = GlobalPosition;
			_tintBlinkElapsed = 0f;
		}

		_rooted = rooted;

		if (!rooted)
			GlobalPosition = _rootBasePosition;   // 흔들림 오프셋 제거하고 목줄 갱신으로 넘김(색은 다음 프레임 발광이 알아서 이어받음)
	}

	// 평상시 HDR 느낌으로 은은하게 발광(흰색↔GlowColor 사인파 왕복). 속박 중엔 그 위를 보라 명멸이 덮는다.
	private void UpdateGlow(double delta)
	{
		if (_sprite == null || _rooted)
			return;

		_glowElapsed += (float)delta;
		float wave = (Mathf.Sin(_glowElapsed * GlowSpeed) + 1f) * 0.5f;
		_sprite.Modulate = Colors.White.Lerp(GlowColor, wave);
	}

	// 스프링 관성은 없지만, 목표 지점까지 지수 감쇠로 부드럽게 좁혀간다(즉시 스냅 X).
	// 속박이 풀린 직후처럼 간격이 크게 벌어져 있던 경우에도 순간이동 없이 자연스럽게 따라붙는다.
	public override void _PhysicsProcess(double delta)
	{
		UpdateGlow(delta);

		if (_rooted)
		{
			_tintBlinkElapsed += (float)delta;
			float wave = (Mathf.Sin(_tintBlinkElapsed * RootTintBlinkSpeed) + 1f) * 0.5f;
			if (_sprite != null)
				_sprite.Modulate = Colors.White.Lerp(RootTintColor, wave);

			GlobalPosition = _rootBasePosition + new Vector2(
				(float)GD.RandRange(-RootShakeMagnitude, RootShakeMagnitude),
				(float)GD.RandRange(-RootShakeMagnitude, RootShakeMagnitude));
			return;
		}

		if (!_leashActive || _leashTarget == null || !IsInstanceValid(_leashTarget))
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

		_sprite?.Play("hit");

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
