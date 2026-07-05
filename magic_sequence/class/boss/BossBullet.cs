// 보스 탄막 한 발. 지정 방향으로 직진하다 타깃과의 거리가 가까워지면 명중 처리.
// 물리 충돌 대신 거리 체크를 쓴다(Monster의 근접 자폭 판정과 같은 방식) — 새 충돌 레이어 불필요.
// 지나온 위치를 Line2D로 기록해 혜성 꼬리 같은 잔상을 남긴다(탄 본체와 별개로 부모에 붙여서, 탄이 사라져도 궤적은 그 자리에 그려져 있던 형태).
public partial class BossBullet : Node2D
{
	private const float HitRadius = 24f;    // 타깃 중심 이 거리 안에 들어오면 명중
	private const float MaxLifetime = 5f;   // 이 시간 넘게 못 맞히면 소멸(화면 밖으로 날아간 탄 정리)
	private const int TrailPointCount = 10;    // 잔상 포인트 개수(길수록 꼬리가 김)
	private const float TrailHeadWidth = 8f;   // 머리(현재 위치) 쪽 두께, 꼬리로 갈수록 얇아짐
	private static readonly Color TrailColor = new Color(1f, 0.5f, 0.8f, 0.6f);

	private Vector2 _direction;
	private float _speed;
	private int _damage;
	private Team _sourceTeam;
	private Node2D _targetNode;   // 명중 대상. 보스 탄막에선 코어(Boss가 TargetNode를 넘겨줌) — Core는 IEntity라 Hit 가능
	private double _life;
	private Line2D _trail;

	// 발사 파라미터 주입. Boss.SpawnBullet에서 방향·속도·데미지·진영·타깃(코어)을 넣는다.
	public void Fire(Vector2 direction, float speed, int damage, Team sourceTeam, Node2D targetNode)
	{
		_direction = direction.Normalized();
		_speed = speed;
		_damage = damage;
		_sourceTeam = sourceTeam;
		_targetNode = targetNode;
		Rotation = _direction.Angle();
	}

	public override void _PhysicsProcess(double delta)
	{
		// Boss.SpawnBullet은 AddChild 직후 GlobalPosition을 스폰 위치로 세팅하므로,
		// _Ready 대신 여기서 첫 틱에 늦게 생성해야 잔상이 원점(0,0)에서 시작하지 않는다.
		if (_trail == null)
			InitTrail();

		// 수명 초과 시 소멸(코어를 스쳐 지나가 영영 안 맞는 탄 정리)
		_life += delta;
		if (_life >= MaxLifetime)
		{
			_trail.QueueFree();
			QueueFree();
			return;
		}

		// 직진 이동
		GlobalPosition += _direction * _speed * (float)delta;
		UpdateTrail();

		// 타깃이 사라졌으면(코어 파괴 등) 명중 판정 스킵하고 계속 날아감
		if (_targetNode == null || !IsInstanceValid(_targetNode))
			return;

		// 타깃과 충분히 가까워지기 전엔 통과 — 회피 여지를 주는 지점(레이저와 달리 피할 수 있음)
		if (GlobalPosition.DistanceTo(_targetNode.GlobalPosition) > HitRadius)
			return;

		// 명중: 타깃(코어)에 데미지 주고 소멸
		if (_targetNode is IEntity entity)
			entity.Hit(new HitInfo { Damage = _damage, SourceTeam = _sourceTeam, Element = Elemental.None });

		_trail.QueueFree();
		QueueFree();
	}

	private void InitTrail()
	{
		_trail = new Line2D();
		_trail.Width = TrailHeadWidth;
		_trail.WidthCurve = BuildTaperCurve();
		_trail.DefaultColor = TrailColor;
		_trail.Gradient = BuildFadeGradient();
		GetParent().AddChild(_trail);
		_trail.GlobalPosition = GlobalPosition;
		_trail.AddPoint(Vector2.Zero);
	}

	private void UpdateTrail()
	{
		_trail.AddPoint(_trail.ToLocal(GlobalPosition));
		if (_trail.GetPointCount() > TrailPointCount)
			_trail.RemovePoint(0);
	}

	// 포인트 0(가장 오래된 꼬리 끝) → 1(방금 찍은 머리 쪽)로 갈수록 두꺼워짐
	private static Curve BuildTaperCurve()
	{
		var curve = new Curve();
		curve.AddPoint(new Vector2(0f, 0f));
		curve.AddPoint(new Vector2(1f, 1f));
		return curve;
	}

	// 꼬리 끝은 투명, 머리 쪽은 원래 색으로 — Line2D 포인트 순서(오래된 것→최신)와 동일한 방향
	private static Gradient BuildFadeGradient()
	{
		var gradient = new Gradient();
		gradient.SetColor(0, new Color(TrailColor.R, TrailColor.G, TrailColor.B, 0f));
		gradient.SetColor(1, TrailColor);
		return gradient;
	}
}
