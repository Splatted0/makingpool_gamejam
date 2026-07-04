// 10웨이브 단독 보스. Monster를 상속해 Health/피격/VFX/디버프/애니를 재사용하되,
// 타깃을 향한 행군은 하지 않고(오른쪽 고정) 코어를 노려 자체 패턴을 시전한다.
// 역할 분담: Boss=몸통+예고선+패턴용 API / BossPatternController=스케줄링 / IBossPattern=한 사이클 동작.
public partial class Boss : Monster
{
	[Export] public PackedScene BulletScene { get; set; }   // BossBullet.cs가 붙은 탄막 프리팹
	[Export] public AnimatedSprite2D DiceSprite { get; set; }   // 얼굴 1~7 = Frame 0~6. Play() 안 쓰고 Frame만 직접 제어
	[Export] public PackedScene MonsterScene { get; set; }   // Monster.cs가 붙은 범용 몬스터 씬(스포너와 동일)
	[Export] public MonsterData ShieldData { get; set; }     // 방패병 데이터(MoveSpeed=0, 원거리형+데미지0 권장)

	private BossData _bossData;
	private Line2D _beam;               // 보스↔코어 상시 빨간 예고선
	private BossPatternController _patterns;

	// 패턴 조각이 참조하는 공개 API
	public BossData Config => _bossData;
	public Vector2 CorePosition =>
		TargetNode != null && IsInstanceValid(TargetNode) ? TargetNode.GlobalPosition : GlobalPosition;

	public void HitCore(int damage)
	{
		Core?.Hit(new HitInfo { Damage = damage, SourceTeam = Team, Element = Elemental.None });
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

	public void SetBeamWidth(float width)
	{
		if (_beam != null)
			_beam.Width = width;
	}

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
	}

	// 씬에 직접 배치된 경우 코어를 그룹에서 찾아 타깃으로 잡는다.
	// Spawner 경유로 소환될 땐 SetTarget이 이미 호출돼 있어 이 경로를 타지 않는다.
	public override void _Ready()
	{
		if (!HasTarget)
			ResolveCoreTarget();

		base._Ready();

		_bossData = Data as BossData;
		SetupBeam();
		SetupDiceSprite();
		_patterns = new BossPatternController(this);
	}

	// 오른쪽 고정. 행군 대신 예고선 갱신 + 패턴 스케줄러 tick.
	protected override void UpdateBehavior(double delta)
	{
		UpdateBeamGeometry();
		_patterns?.Tick(delta);
	}

	// 스턴 상태이상 재해석: 레이저 차지 캔슬(예고선은 계속 코어 추적).
	protected override void OnStunned(float delta)
	{
		UpdateBeamGeometry();
		_patterns?.OnStunned();
	}

	private void SetupBeam()
	{
		_beam = new Line2D();
		_beam.DefaultColor = new Color(1f, 0.15f, 0.25f, 0.85f);
		_beam.Width = _bossData != null ? _bossData.LaserWidthIdle : 2f;
		_beam.AddPoint(Vector2.Zero);   // 보스 원점
		_beam.AddPoint(Vector2.Zero);   // 코어(매 프레임 갱신)
		AddChild(_beam);
	}

	// autoplay로 SpriteFrames가 자체 재생하는 걸 막고, SetDiceFace로만 프레임을 제어한다.
	private void SetupDiceSprite()
	{
		if (DiceSprite == null)
			return;

		DiceSprite.Stop();
		DiceSprite.Frame = 0;
	}

	private void UpdateBeamGeometry()
	{
		if (_beam != null)
			_beam.SetPointPosition(1, ToLocal(CorePosition));
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
