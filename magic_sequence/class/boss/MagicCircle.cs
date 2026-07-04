// 지면 장판(마법진) 1개. 예고(Telegraph) 동안 반투명하게 표시되다가 활성화되는 순간
// 그 자리에 있던 타깃에 데미지를 준 뒤, 서서히 투명해지며 사라진다.
// BossBullet과 마찬가지로 콜리전 대신 거리 체크를 쓴다(새 충돌 레이어 불필요).
public partial class MagicCircle : Node2D
{
	[Export] public Sprite2D Visual { get; set; }

	private const float SourceTextureRadius = 32f; // 스프라이트 원본(64px) 기준 중심→테두리 반지름. Scale 환산용.
	private const float TelegraphAlpha = 0.35f;

	// 알파만 올리면 원본 스프라이트가 옅어서 안 진해 보임 — 채널 값을 1 이상으로 올려 밝기 자체를 태워서 확 튀게 한다.
	private static readonly Color ActiveColor = new Color(2f, 2f, 2f, 1f);

	private float _radius;
	private float _telegraphDuration;
	private float _activeDuration;
	private int _damage;
	private Team _sourceTeam;
	private Node2D _targetNode;

	private float _elapsed;
	private bool _active;

	// 소환 직후 호출. radius는 실제 판정 반지름(px). telegraphDuration 동안 예고만 하다가
	// activeDuration 동안 활성화 표시를 유지한 뒤 사라진다. targetNode는 판정 대상(IEntity 구현체).
	public void Setup(float radius, float telegraphDuration, float activeDuration, int damage, Team sourceTeam, Node2D targetNode)
	{
		_radius = radius;
		_telegraphDuration = telegraphDuration;
		_activeDuration = activeDuration;
		_damage = damage;
		_sourceTeam = sourceTeam;
		_targetNode = targetNode;

		if (Visual != null)
		{
			Visual.Scale = Vector2.One * (_radius / SourceTextureRadius);
			Visual.Modulate = new Color(1f, 1f, 1f, TelegraphAlpha);
		}
	}

	// 예고 동안은 반투명으로 고정 표시. 활성화되는 순간 판정하고 밝게 확 켜졌다가
	// activeDuration에 걸쳐 서서히 투명해지며 사라진다(BossLaserSprayBeam과 같은 페이드 방식).
	public override void _Process(double delta)
	{
		_elapsed += (float)delta;

		if (!_active)
		{
			if (_elapsed < _telegraphDuration)
				return;

			_active = true;
			_elapsed = 0f;
			TryHit();
			return;
		}

		float t = Mathf.Clamp(_elapsed / _activeDuration, 0f, 1f);
		if (Visual != null)
		{
			Color color = ActiveColor;
			color.A = Mathf.Lerp(ActiveColor.A, 0f, t);
			Visual.Modulate = color;
		}

		if (_elapsed >= _activeDuration)
			QueueFree();
	}

	// 활성화되는 순간 1회만 판정(그 자리에 남아있으면 맞고, 피했으면 안 맞는다)
	private void TryHit()
	{
		if (_targetNode == null || !IsInstanceValid(_targetNode))
			return;

		if (GlobalPosition.DistanceTo(_targetNode.GlobalPosition) > _radius)
			return;

		if (_targetNode is IEntity entity)
			entity.Hit(new HitInfo { Damage = _damage, SourceTeam = _sourceTeam, Element = Elemental.None });
	}
}
