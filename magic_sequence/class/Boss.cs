// 10웨이브 단독 보스. Monster를 상속해 Health/피격/VFX/디버프/애니를 재사용하되,
// 타깃을 향한 행군은 하지 않고(오른쪽 고정) 코어를 노려 자체 패턴을 시전한다.
// 역할 분담: Boss=몸통+예고선+패턴용 API / BossPatternController=스케줄링 / IBossPattern=한 사이클 동작.
public partial class Boss : Monster
{
	[Export] public PackedScene BulletScene { get; set; }   // BossBullet.cs가 붙은 탄막 프리팹
	[Export] public AnimatedSprite2D DiceSprite { get; set; }   // 얼굴 1~7 = Frame 0~6. Play() 안 쓰고 Frame만 직접 제어
	[Export] public PackedScene MonsterScene { get; set; }   // Monster.cs가 붙은 범용 몬스터 씬(스포너와 동일)
	[Export] public MonsterData ShieldData { get; set; }     // 방패병 데이터(MoveSpeed=0, 원거리형+데미지0 권장)
	[Export] public PackedScene MagicCircleScene { get; set; }   // MagicCircle.cs가 붙은 장판 프리팹
	[Export] public Gradient LaserGradient { get; set; }     // 레이저 색상(에디터에서 그라데이션 편집, 주사위6)

	private const string ShieldGroup = "boss_shield";

	private BossData _bossData;
	private BossPatternController _patterns;
	private BossAnimator _bossAnimator;  // idle 기본 + 패턴 이벤트 시 단발 애니(die는 상속받은 MonsterAnimator가 처리)
	private float _groundZoneElapsed;    // 주사위와 무관한 기본 공격(장판) 주기 타이머

	// 패턴 조각이 참조하는 공개 API
	public BossData Config => _bossData;
	public Vector2 CorePosition =>
		TargetNode != null && IsInstanceValid(TargetNode) ? TargetNode.GlobalPosition : GlobalPosition;

	public void HitCore(int damage)
	{
		Core?.Hit(new HitInfo { Damage = damage, SourceTeam = Team, Element = Elemental.None });
	}

	public void SetCoreRooted(bool rooted)
	{
		if (Core is global::Core core)
			core.SetRooted(rooted);
	}

	public void SpawnBullet(Vector2 direction, float speed, int damage)
	{
		if (BulletScene == null)
		{
			GD.PrintErr($"[Boss] {Name}: BulletScene이 비어있습니다.");
			return;
		}

		BossBullet bullet = BulletScene.Instantiate<BossBullet>();
		GetParent().AddChild(bullet);
		bullet.GlobalPosition = GlobalPosition;
		bullet.Fire(direction, speed, damage, Team, TargetNode);
	}

	// 주사위와 무관하게 일정 주기로 플레이어 근방에 장판(마법진) 여러 개를 동시에 깐다(기본 공격).
	private void UpdateGroundZoneAttack(double delta)
	{
		if (_bossData == null)
			return;

		_groundZoneElapsed += (float)delta;
		if (_groundZoneElapsed < _bossData.GroundZoneInterval)
			return;

		_groundZoneElapsed = 0f;
		SpawnGroundZones();
	}

	private void SpawnGroundZones()
	{
		if (MagicCircleScene == null)
		{
			GD.PrintErr($"[Boss] {Name}: MagicCircleScene이 비어있습니다.");
			return;
		}

		Node2D player = GetTree().GetFirstNodeInGroup("player") as Node2D;
		Vector2 center = player != null && IsInstanceValid(player) ? player.GlobalPosition : GlobalPosition;

		// 순수 랜덤 좌표만으로는 좁은 범위에 여러 개를 욱여넣다 계속 겹쳐서, 각자 몫의 각도 구간(부채꼴)을
		// 먼저 배정하고 그 안에서만 흔들어 배치한다 — 구조적으로 서로 겹치기 어렵게 만드는 방식.
		int count = _bossData.GroundZoneCount;
		float sectorDegrees = 360f / count;
		float minDistance = _bossData.GroundZoneSpreadRange * 0.4f;
		const int maxAttemptsPerZone = 30;
		var placed = new Vector2[count];

		for (int i = 0; i < count; i++)
		{
			Vector2 position = Vector2.Zero;
			for (int attempt = 0; attempt < maxAttemptsPerZone; attempt++)
			{
				float angleDeg = i * sectorDegrees + (float)GD.RandRange(-sectorDegrees * 0.4, sectorDegrees * 0.4);
				float distance = (float)GD.RandRange(minDistance, _bossData.GroundZoneSpreadRange);
				position = center + Vector2.Right.Rotated(Mathf.DegToRad(angleDeg)) * distance;

				bool tooClose = false;
				for (int j = 0; j < i; j++)
				{
					if (position.DistanceTo(placed[j]) < _bossData.GroundZoneMinSpacing)
					{
						tooClose = true;
						break;
					}
				}

				if (!tooClose)
					break;
			}

			placed[i] = position;

			MagicCircle circle = MagicCircleScene.Instantiate<MagicCircle>();
			Node parent = Blackboard.Main != null ? (Node)Blackboard.EntityContainer : GetParent();
			parent.AddChild(circle);
			circle.GlobalPosition = position;
			circle.Setup(_bossData.GroundZoneRadius, _bossData.GroundZoneTelegraphDuration, _bossData.GroundZoneActiveDuration, _bossData.GroundZoneDamage, Team, TargetNode);
		}

		GD.Print($"[GroundZone] 장판 {_bossData.GroundZoneCount}개 소환");
	}

	public void PlayAttackAnim() => _bossAnimator?.PlayAttack();
	public void PlayBuffAnim() => _bossAnimator?.PlayBuff();
	public void PlayDebuffAnim() => _bossAnimator?.PlayDebuff();
	public void PlayDiceRollAnim() => _bossAnimator?.PlayDiceRoll();
	public void PlayLuckyAnim() => _bossAnimator?.PlayLucky();

	public void SetDiceFace(int face)
	{
		if (DiceSprite != null)
			DiceSprite.Frame = face - 1;
	}

	// 스포너와 동일한 순서(Data → SetTarget → AddChild)로 방패병 한 마리를 즉석 소환한다.
	public void SummonShield(Vector2 worldPosition)
	{
		if (MonsterScene == null || ShieldData == null)
		{
			GD.PrintErr($"[Boss] {Name}: MonsterScene/ShieldData가 비어있습니다.");
			return;
		}

		Monster shield = MonsterScene.Instantiate<Monster>();
		shield.Data = ShieldData;
		if (TargetNode != null && IsInstanceValid(TargetNode) && Core != null)
			shield.SetTarget(TargetNode.GlobalPosition, Core);

		// TODO(M6 통합 시 롤백): GetParent() 폴백은 독립 테스트 씬 디버깅용 임시 조치.
		// Blackboard.EntityContainer는 내부적으로 Main.BattleWorldHud를 타는데, Main이 없는
		// 독립 씬에서는 그 자체가 NullReferenceException을 던져서 임시로 우회해둔 것.
		// 실제 battle_world 통합 후엔 Blackboard.EntityContainer만 쓰도록 되돌릴 것.
		Node parent = Blackboard.Main != null ? (Node)Blackboard.EntityContainer : GetParent();
		parent.AddChild(shield);
		shield.GlobalPosition = worldPosition;
		shield.AddToGroup(ShieldGroup);
	}

	// 이전에 소환된 방패병을 전부 정리한다. 재소환 직전에 호출해 벽이 계속 쌓이지 않게 한다.
	public void ClearShields()
	{
		foreach (Node node in GetTree().GetNodesInGroup(ShieldGroup))
			node.QueueFree();
	}

	// 씬에 직접 배치된 경우 코어를 그룹에서 찾아 타깃으로 잡는다.
	// Spawner 경유로 소환될 땐 SetTarget이 이미 호출돼 있어 이 경로를 타지 않는다.
	public override void _Ready()
	{
		if (!HasTarget)
			ResolveCoreTarget();

		base._Ready();

		_bossData = Data as BossData;
		SetupDiceSprite();
		StartCoreLeash();
		_bossAnimator = new BossAnimator(AnimatedSprite);
		_patterns = new BossPatternController(this);
	}

	// 보스 등장 자체가 "웨이브10 시작" 신호 — 별도 이벤트 없이 여기서 코어를 플레이어에 붙인다.
	private void StartCoreLeash()
	{
		if (Core is not global::Core core)
			return;

		Node2D player = GetTree().GetFirstNodeInGroup("player") as Node2D;
		core.StartLeash(player);
	}

	// 오른쪽 고정. 행군 대신 패턴 스케줄러 tick. 기본 애니는 idle(단발 재생 중이면 무시됨).
	protected override void UpdateBehavior(double delta)
	{
		_bossAnimator?.PlayIdle();
		_patterns?.Tick(delta);
		UpdateGroundZoneAttack(delta);
	}

	// 스턴 상태이상 재해석: debuff 애니만 재생하고, 주사위 굴림·시전은 스턴과 무관하게 계속 진행한다.
	protected override void OnStunned(float delta)
	{
		_bossAnimator?.PlayDebuff();
		_patterns?.Tick(delta);
		UpdateGroundZoneAttack(delta);
	}

	// autoplay로 SpriteFrames가 자체 재생하는 걸 막고, SetDiceFace로만 프레임을 제어한다.
	private void SetupDiceSprite()
	{
		if (DiceSprite == null)
			return;

		DiceSprite.Stop();
		DiceSprite.Frame = 0;
	}

	protected override void Die()
	{
		// 보스 사망 = 승리. 연출·라운드 종료 신호는 통합 단계(M6)에서. 지금은 로그 훅만.
		GD.Print("[Boss] 처치됨 — 승리");
		base.Die();
	}

	private void ResolveCoreTarget()
	{
		Node core = GetTree().GetFirstNodeInGroup("core");
		if (core is Node2D node && core is IEntity entity)
			SetTarget(node.GlobalPosition, entity);
	}
}
