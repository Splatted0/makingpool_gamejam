// 10웨이브 단독 보스. Monster를 상속해 Health/피격/VFX/디버프/애니를 재사용하되,
// 타깃을 향한 행군은 하지 않고(오른쪽 고정) 코어를 노려 자체 패턴을 시전한다.
// 역할 분담: Boss=몸통+예고선+패턴용 API / BossPatternController=스케줄링 / IBossPattern=한 사이클 동작.
public partial class Boss : Monster
{
	[Export] public PackedScene BulletScene { get; set; }   // BossBullet.cs가 붙은 탄막 프리팹

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

	// 씬에 직접 배치된 경우 코어를 그룹에서 찾아 타깃으로 잡는다.
	// Spawner 경유로 소환될 땐 SetTarget이 이미 호출돼 있어 이 경로를 타지 않는다.
	public override void _Ready()
	{
		if (!HasTarget)
			ResolveCoreTarget();

		base._Ready();

		_bossData = Data as BossData;
		SetupBeam();
		_patterns = new BossPatternController(this);
	}

	// 오른쪽 고정. 행군 대신 예고선 갱신 + 패턴 스케줄러 tick.
	protected override void UpdateBehavior(double delta)
	{
		UpdateBeamGeometry();
		_patterns?.Tick(delta);
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
