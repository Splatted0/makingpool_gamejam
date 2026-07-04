// 보스 탄막 한 발. 지정 방향으로 직진하다 타깃과의 거리가 가까워지면 명중 처리.
// 물리 충돌 대신 거리 체크를 쓴다(Monster의 근접 자폭 판정과 같은 방식) — 새 충돌 레이어 불필요.
public partial class BossBullet : Node2D
{
	private const float HitRadius = 24f;    // 타깃 중심 이 거리 안에 들어오면 명중
	private const float MaxLifetime = 5f;   // 이 시간 넘게 못 맞히면 소멸(화면 밖으로 날아간 탄 정리)

	private Vector2 _direction;
	private float _speed;
	private int _damage;
	private Team _sourceTeam;
	private Node2D _targetNode;   // 명중 대상. 보스 탄막에선 코어(Boss가 TargetNode를 넘겨줌) — Core는 IEntity라 Hit 가능
	private double _life;

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
		// 수명 초과 시 소멸(코어를 스쳐 지나가 영영 안 맞는 탄 방지)
		_life += delta;
		if (_life >= MaxLifetime)
		{
			QueueFree();
			return;
		}

		// 직진 이동
		GlobalPosition += _direction * _speed * (float)delta;

		// 타깃이 사라졌으면(코어 파괴 등) 명중 판정 스킵하고 계속 날아감
		if (_targetNode == null || !IsInstanceValid(_targetNode))
			return;

		// 타깃과 충분히 가까워지기 전엔 통과 — 회피 여지를 주는 지점(레이저와 달리 피할 수 있음)
		if (GlobalPosition.DistanceTo(_targetNode.GlobalPosition) > HitRadius)
			return;

		// 명중: 타깃(코어)에 데미지 주고 소멸
		if (_targetNode is IEntity entity)
			entity.Hit(new HitInfo { Damage = _damage, SourceTeam = _sourceTeam, Element = Elemental.None });

		QueueFree();
	}
}
