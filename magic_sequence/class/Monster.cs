// 몬스터 베이스 클래스. 스탯·외형은 MonsterData 리소스에서, 행동(이동/공격/피격)은 여기서.
// 스탯만 다른 종류는 .tres 교체로 끝. 특수 행동을 가진 종류만 이 클래스를 상속해 override 한다.
public partial class Monster : CharacterBody2D, IEntity
{
	[Export] public MonsterData Data { get; set; }   // 종류별 스탯·외형. 인스펙터에서 .tres를 갈아끼운다
	[Export] private AnimatedSprite2D _animatedSprite;   // 외형·애니메이션. 씬에서 연결

	// ── IEntity ──
	public Team Team { get; set; } = Team.Enemy;	// 몬스터는 Team.Enemy, 플레이어는 Team.Player
	public int Health { get; set; }                  // 런타임 현재 체력. _Ready에서 Data.MaxHealth로 초기화

	private const float MELEE_ATTACK_RANGE = 50f;       // melee 공격 판정 거리. Core 콜리전 반지름(30)+몬스터 콜리전 반지름(10)보다 여유 있게

	private bool _hasTarget;                          // 타깃이 정해졌는지 (Core)
	private Vector2 _targetPosition;                 // 향할 Core 좌표. 스포너가 스폰 직후 1회 주입(SetTarget)
	private Vector2 _direction;                       // 스폰 시 1회 계산한 직선 방향(Core 고정이라 갱신 안 함)
	private IEntity _core;                            // 공격 대상. 스포너가 SetTarget으로 주입
	private bool _isAttacking;                        // 사거리 안에 들어와 공격 중인지
	private double _attackTimer;                      // 다음 공격까지 누적 시간

	// ── 디버프 표면 ── 디버프 클래스들이 발동/종료 시 여기를 바꾼다. 적용/만료 관리는 컨트롤러 담당.
	public bool IsStunned { get; set; }                     // 스턴 중이면 이동·공격 정지
	public float MoveSpeedMultiplier { get; set; } = 1f;    // 슬로우 등 이동속도 배율
	public float DamageTakenMultiplier { get; set; } = 1f;  // 최종뎀 디버프(후속)용 배율

	private DebuffController _debuffs;                       // 걸린 디버프 관리

	// 근거리 여부 — Data.AttackRange가 음수면 접촉 공격
	protected bool IsMelee
	{
		get
		{
			if (Data.AttackRange < 0f)
			{
				return true;
			}
			return false;
		}
	}

	// 스포너가 스폰 직후 Core 좌표·객체를 넘긴다(주입 방식)
	public void SetTarget(Vector2 target, IEntity core)
	{
		_targetPosition = target;
		_core = core;
		_hasTarget = true;
	}

	public void Hit(HitInfo hitInfo)
	{
		if (hitInfo.SourceTeam == Team)
			return;

		TakeDamage(hitInfo.Damage);

		// 원소로 대응 디버프 적용. 콤보(다중 원소)는 HitInfo가 리스트가 되면 순회로 확장.
		IDebuff debuff = DebuffController.Create(hitInfo.Element);
		if (debuff != null)
		{
			_debuffs.Apply(debuff);
		}
	}

	// 피격. 받은 데미지에 최종뎀 배율 적용 후 체력 감소.
	public virtual void TakeDamage(int amount)
	{
		Health -= Mathf.RoundToInt(amount * DamageTakenMultiplier);
		if (Health <= 0)
		{
			Die();
		}
	}

	public override void _Ready()
	{
		_debuffs = new DebuffController(this);

		if (Data == null)
		{
			GD.PrintErr($"[Monster] {Name}: Data가 비어있습니다. 인스펙터에서 MonsterData를 지정해야 합니다.");
			return;
		}

		Health = Data.MaxHealth;

		if (_animatedSprite != null && Data.Frames != null)
		{
			_animatedSprite.SpriteFrames = Data.Frames;
			_animatedSprite.Play();
		}

		// Core 고정이라 방향은 여기서 한 번만 계산 → 이후 직선 유지
		if (_hasTarget)
		{
			_direction = (_targetPosition - GlobalPosition).Normalized();
		}
		else
		{
			GD.PrintErr($"[Monster] {Name}: SetTarget이 호출되지 않았습니다. 스포너가 AddChild 전에 SetTarget을 호출해야 합니다.");
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		_debuffs.Tick((float)delta);   // 지속 감소·틱데미지·만료(스턴/슬로우 해제)를 먼저 반영

		if (IsStunned)
		{
			Velocity = Vector2.Zero;
			MoveAndSlide();
			return;
		}

		float distance = GlobalPosition.DistanceTo(_targetPosition);

		if (IsMelee)
		{
			MeleeUpdate(delta, distance);
		}
		else
		{
			RangedUpdate(delta, distance);
		}
	}

	// 근거리: Core에 닿으면 남은 체력만큼 데미지 주고 자폭. 닿기 전엔 직선 이동.
	// 자폭은 처치가 아니라 본진 관통이므로 골드 없이 QueueFree(Die 아님).
	private void MeleeUpdate(double delta, float distance)
	{
		if (distance <= MELEE_ATTACK_RANGE)
		{
			_core.Hit(new HitInfo
			{
				Damage = Health,
				SourceTeam = Team,
				Element = Elemental.None
			});
			QueueFree();
			return;
		}
		Move(delta);
	}

	// 원거리: 사거리 안에서 멈춰 AttackInterval마다 반복 공격.
	private void RangedUpdate(double delta, float distance)
	{
		if (distance <= Data.AttackRange)
		{
			if (_isAttacking == false)
			{
				_isAttacking = true;
				_attackTimer = 0;
				Velocity = Vector2.Zero;
				MoveAndSlide();
			}
			AttackInterval(delta);
		}
		else
		{
			_isAttacking = false;
			Move(delta);
		}
	}

	// 이동 담당. 기본은 스폰 지점에서 Core로 직선 이동.
	// 특수 이동 패턴을 가진 몬스터는 이 메서드를 override 한다.
	protected virtual void Move(double delta)
	{
		Velocity = _direction * Data.MoveSpeed * MoveSpeedMultiplier;
		MoveAndSlide();
	}

	// 사거리 안에서 AttackInterval마다 한 번씩 Attack 호출
	private void AttackInterval(double delta)
	{
		_attackTimer += delta;
		if (_attackTimer >= Data.AttackInterval)
		{
			_attackTimer = 0;
			Attack();
		}
	}

	// Core 공격. 근/원거리는 Data.AttackRange(IsMelee)로 갈린다.
	// 공격 연출이 다른 몬스터는 이 메서드를 override 한다.
	protected virtual void Attack()
	{
		_core.Hit(new HitInfo
		{
			Damage = Data.AttackDamage,
			SourceTeam = Team,
			Element = Elemental.None
		});
	}

	// 사망 처리. 이펙트·드롭 등이 필요하면 override
	protected virtual void Die()
	{
		if (Data != null)
		{
			Blackboard.AddGold(Data.GoldReward);
		}

		QueueFree();
	}
}
