public partial class Monster : CharacterBody2D, IEntity
{
	[Export] public MonsterData Data { get; set; }   // 종류별 스탯·외형. 인스펙터에서 .tres를 갈아끼운다
	[Export] private AnimatedSprite2D _animatedSprite;   // 외형·애니메이션. 씬에서 연결

	// ── IEntity ──
	public Team Team { get; set; } = Team.Enemy;	// 몬스터는 Team.Enemy, 플레이어는 Team.Player
	public int Health { get; set; }                  // 런타임 현재 체력. _Ready에서 Data.MaxHealth로 초기화

	private const float MELEE_ATTACK_RANGE = 50f;       // melee 공격 판정 거리. Core 콜리전 반지름(30)+몬스터 콜리전 반지름(10)보다 여유 있게

	private const int MELEE_COLLISION_LAYER = 5;        // 근거리끼리 충돌용 물리 레이어(1·2 마법, 4 몬스터와 겹치지 않게)
	private const int RANGED_COLLISION_LAYER = 6;       // 원거리끼리 충돌용 물리 레이어

	private bool _hasTarget;                          // 타깃이 정해졌는지 (Core)
	private Vector2 _targetPosition;                 // 향할 Core 좌표. 스포너가 스폰 직후 1회 주입(SetTarget)
	private Vector2 _direction;                       // 스폰 시 1회 계산한 직선 방향(Core 고정이라 갱신 안 함)
	private IEntity _core;                            // 공격 대상. 스포너가 SetTarget으로 주입
	private bool _isAttacking;                        // 사거리 안에 들어와 공격 중인지
	private double _attackTimer;                      // 다음 공격까지 누적 시간

	private const float KNOCKBACK_DECAY = 1800f;       // 넉백 감속량(px/s). 클수록 빨리 멈춤
	private Vector2 _knockbackVelocity;               // 남은 넉백 속도. 0이 아니면 넉백 중, 매 프레임 0으로 감쇠

	// ── 디버프 표면 ── 디버프 클래스들이 발동/종료 시 여기를 바꾼다. 적용/만료 관리는 컨트롤러 담당.
	public bool IsStunned { get; set; }                     // 스턴 중이면 이동·공격 정지
	public float MoveSpeedMultiplier { get; set; } = 1f;    // 슬로우 등 이동속도 배율
	public float DamageTakenMultiplier { get; set; } = 1f;  // 최종뎀 디버프(후속)용 배율

	private DebuffController _debuffs;                       // 걸린 디버프 관리
	private MonsterAnimator _animator;                       // 애니메이션 재생 담당
	private bool _isDying;                                   // 사망 애니 재생 중이면 이동·공격·넉백 정지

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

		TakeDamage(hitInfo.Damage, ColorPreset.White);

		// 원소로 대응 디버프 적용. 콤보(다중 원소)는 HitInfo가 리스트가 되면 순회로 확장.
		IDebuff debuff = DebuffController.Create(hitInfo.Element);
		if (debuff != null)
		{
			_debuffs.Apply(debuff);
		}
	}

	// 피격 + 데미지 팝업 색 지정. 일반 피해는 흰색, 화상 등 지속뎀은 색을 다르게 넘겨 종류를 구분한다.
	public virtual void TakeDamage(int amount, Color color)
	{
		// 이미 죽는 중이면 무시 — 화상 등 지속뎀이 자폭/사망 시퀀스를 끊는 것 방지
		if (_isDying)
		{
			return;
		}

		int damage = Mathf.RoundToInt(amount * DamageTakenMultiplier);
		Health -= damage;

		// 데미지 팝업(잃은 체력 표시, 색으로 피해 종류 구분)
		Vfx.ExplanationDamage.Throw(new VfxExplanationDamageData
		{
			GlobalPosition = GlobalPosition,
			Damage = damage,
			Color = color
		});

		if (Health <= 0)
		{
			Die();
			return;
		}
		_animator.PlayHit();
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

		// 근거리끼리 / 원거리끼리만 충돌하고, 근↔원은 서로 통과.
		// 타입별 레이어에 소속시키고 마스크를 자기 타입만으로 잡는다(기존 레이어=마법 감지 등은 유지).
		int typeLayer = RANGED_COLLISION_LAYER;
		if (IsMelee)
		{
			typeLayer = MELEE_COLLISION_LAYER;
		}
		SetCollisionLayerValue(typeLayer, true);
		CollisionMask = 0;
		SetCollisionMaskValue(typeLayer, true);

		if (_animatedSprite != null && Data.Frames != null)
		{
			_animatedSprite.SpriteFrames = Data.Frames;
		}

		_animator = new MonsterAnimator(_animatedSprite);

		// Core 고정이라 방향은 여기서 한 번만 계산 → 이후 직선 유지
		if (_hasTarget)
		{
			_direction = (_targetPosition - GlobalPosition).Normalized();
			_animator.PlayWalk();
		}
		else
		{
			GD.PrintErr($"[Monster] {Name}: SetTarget이 호출되지 않았습니다. 스포너가 AddChild 전에 SetTarget을 호출해야 합니다.");
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		_debuffs.Tick((float)delta);   // 지속 감소·틱데미지·만료(스턴/슬로우 해제)를 먼저 반영

		// 사망 애니 재생 중엔 아무 행동도 하지 않고 멈춘다(die 애니가 끝나면 QueueFree).
		if (_isDying)
		{
			Velocity = Vector2.Zero;
			MoveAndSlide();
			return;
		}

		// 넉백 중에는 전진·공격을 멈추고 넉백만 적용한다(밀려나는 게 온전히 보이게).
		// 매 프레임 속도를 0으로 감쇠시키고, 0에 도달하면 아래 일반 로직으로 복귀한다.
		// 스턴보다 우선 — 짧은 충격이라 스턴에 걸려도 밀려나긴 함.
		if (_knockbackVelocity != Vector2.Zero)
		{
			Velocity = _knockbackVelocity;
			MoveAndSlide();
			_knockbackVelocity = _knockbackVelocity.MoveToward(Vector2.Zero, KNOCKBACK_DECAY * (float)delta);
			return;
		}

		if (IsStunned)
		{
			Velocity = Vector2.Zero;
			MoveAndSlide();
			return;
		}

		// 힐러(리치)는 이동·공격과 별개로 HEAL_INTERVAL마다 주변 아군을 회복
		if (IsHealer)
		{
			HealTick(delta);
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

	// 근거리: Core에 닿으면 남은 체력만큼 데미지 주고, 공격→사망 애니 후 파괴. 닿기 전엔 직선 이동.
	// 자폭은 처치가 아니라 본진 관통이므로 골드 없이 QueueFree(Die 아님).
	private void MeleeUpdate(double delta, float distance)
	{
		if (distance <= MELEE_ATTACK_RANGE)
		{
			_isDying = true;   // 이후 프레임은 _PhysicsProcess 상단에서 정지(재진입 방지)
			DisableCollision();
			_core.Hit(new HitInfo
			{
				Damage = Health,
				SourceTeam = Team,
				Element = Elemental.None
			});
			_animator.PlaySelfDestruct(QueueFree);
			return;
		}
		Move(delta);
	}

	// 원거리: 사거리 안에서 멈춰 AttackInterval마다 반복 공격.
	private void RangedUpdate(double delta, float distance)
	{
		if (distance <= Data.AttackRange)
		{
			_animator.PlayAttack();   // 매 프레임 호출해도 loop 가드로 안전(피격 후 복귀도 여기서)
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
		_animator.PlayWalk();   // 매 프레임 호출해도 loop 가드로 안전(피격 후 복귀도 여기서)
		Velocity = _direction * Data.MoveSpeed * MoveSpeedMultiplier;
		MoveAndSlide();
	}

	// 넉백. 진행 방향(_direction) 반대로 초기 속도 amount(px/s)를 실어준다.
	// 방향은 파라미터로 받지 않고 내부에서 결정 — 항상 Core 반대쪽으로 후퇴.
	// 실제 이동·감쇠는 _PhysicsProcess의 넉백 블록이 처리한다(여기선 속도만 세팅).
	public void Knockback(float amount)
	{
		_knockbackVelocity = -_direction * amount;
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

	// 사망 처리. 사망 애니를 재생하고, 애니가 끝나면 QueueFree.
	// 이펙트·드롭 등이 필요하면 override
	protected virtual void Die()
	{
		if (_isDying)
		{
			return;
		}
		_isDying = true;
		DisableCollision();

		// 죽음 이펙트
		Vfx.DeathParticle.Throw(new VfxDeathParticleData
		{
			GlobalPosition = GlobalPosition
		});

		if (Data != null)
		{
			Blackboard.AddGold(Data.GoldReward);
		}

		_animator.PlayDeath(QueueFree);
	}

	// 사망/자폭 진입 시 콜라이더 해제 — 죽어가는 몹끼리 서로 밀지 않고 그냥 겹치게.
	// 물리 콜백 중 즉시 변경은 에러라 SetDeferred로 다음 프레임에 안전하게 끈다.
	private void DisableCollision()
	{
		CollisionShape2D shape = GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
		shape?.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
	}











	/////////////////////////////////////////////////////////////////////////////
	// 힐러(리치) 전용 로직 — 힐 관련 코드 전부 여기 모음.
	// 호출 지점은 _PhysicsProcess의 `if (IsHealer) HealTick(delta);` 한 곳뿐.
	/////////////////////////////////////////////////////////////////////////////

	private const float HEAL_INTERVAL = 1f;    // 힐 발동 간격(초). 리치만 써서 코드 상수로 고정
	private const float HEAL_RANGE = 400f;     // 힐 사거리(px). 이 안의 다른 몬스터를 회복
	private double _healTimer;                  // 다음 힐까지 누적 시간

	// 힐러 여부 — Data.HealAmount가 양수면 주기적으로 주변 아군을 회복(리치)
	protected bool IsHealer
	{
		get
		{
			if (Data.HealAmount > 0)
			{
				return true;
			}
			return false;
		}
	}

	// 힐러(리치) 전용. HEAL_INTERVAL마다 사거리 내 다른 몬스터를 회복한다.
	private void HealTick(double delta)
	{
		_healTimer += delta;
		if (_healTimer < HEAL_INTERVAL)
		{
			return;
		}
		_healTimer = 0;
		HealNearbyAllies();
	}

	// EntityContainer의 몬스터 중 자신을 뺀 사거리 내 대상을 Data.HealAmount만큼 회복.
	// 몬스터는 Core·Player·Projectiles와 함께 EntityContainer의 직접 자식으로 붙는다.
	private void HealNearbyAllies()
	{
		Node2D container = Blackboard.EntityContainer;
		if (container == null)
		{
			return;
		}

		foreach (Node node in container.GetChildren())
		{
			if (node is Monster ally && ally != this && ally._isDying == false)
			{
				float distance = GlobalPosition.DistanceTo(ally.GlobalPosition);
				if (distance <= HEAL_RANGE)
				{
					ally.Heal(Data.HealAmount);
				}
			}
		}
	}

	// 힐러에게 회복받을 때. MaxHealth를 넘지 않게 캡.
	public void Heal(int amount)
	{
		if (amount <= 0)
		{
			return;
		}
		Health += amount;
		if (Health > Data.MaxHealth)
		{
			Health = Data.MaxHealth;
		}
	}
}
