// 10웨이브 단독 보스. Monster를 상속해 Health/피격/VFX/디버프/애니를 재사용하되,
// 타깃을 향한 행군은 하지 않고(오른쪽 고정) 코어를 노려 자체 패턴을 시전한다.
// 역할 분담: Boss=몸통+예고선+패턴용 API / BossPatternController=스케줄링 / IBossPattern=한 사이클 동작.
public partial class Boss : Monster
{
	[Export] public AnimatedSprite2D DiceSprite { get; set; }   // 얼굴 1~7 = Frame 0~6. Play() 안 쓰고 Frame만 직접 제어

	private const string ShieldGroup = "boss_shield";

	private BossData _bossData;
	private BossPatternController _patterns;
	private BossAnimator _bossAnimator;  // idle 기본 + 패턴 이벤트 시 단발 애니(die는 상속받은 MonsterAnimator가 처리)
	private float _groundZoneElapsed;    // 주사위와 무관한 기본 공격(장판) 주기 타이머
	private Vector2 _diceBasePosition;   // 주사위 스프라이트 원래 위치(굴릴 때 흔들고 끝나면 여기로 복원)
	private Vector2 _diceBaseScale;      // 주사위 스프라이트 원래 크기(버프 중 커졌다가 끝나면 여기로 복원)
	private float _diceFloatElapsed;     // 평상시 위아래로 둥실거리는 아이들 모션 타이머

	private bool _tintFlashing;
	private float _tintFlashElapsed;
	private float _tintFlashDuration;
	private Color _tintFlashColor;

	private bool _tintBlinking;
	private float _tintBlinkElapsed;
	private float _tintBlinkSpeed;
	private Color _tintBlinkColor;

	// 패턴 조각이 참조하는 공개 API
	public BossData Config => _bossData;
	public Vector2 CorePosition =>
		TargetNode != null && IsInstanceValid(TargetNode) ? TargetNode.GlobalPosition : GlobalPosition;

	public void HitCore(int damage)
	{
		Core?.Hit(new HitInfo { Damage = damage, SourceTeam = Team, Element = Elemental.None });
	}

	// TODO(디버그 임시): 보스 체력이 너무 빨리 깎이는 원인 파악용, 확인 후 제거
	public override void TakeDamage(int amount, Color color)
	{
		base.TakeDamage(amount, color);
		GD.Print($"[Boss 디버그] 피격 {amount} → Health {Health}/{Data.MaxHealth}");
	}

	public void SetCoreRooted(bool rooted)
	{
		if (Core is global::Core core)
			core.SetRooted(rooted);
	}

	public void SpawnBullet(Vector2 direction, float speed, int damage)
	{
		if (_bossData?.BulletScene == null)
		{
			GD.PrintErr($"[Boss] {Name}: BulletScene이 비어있습니다.");
			return;
		}

		BossBullet bullet = _bossData.BulletScene.Instantiate<BossBullet>();
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

	// 보스 스프라이트를 잠깐 특정 색으로 확 틴트했다가 원래 흰색으로 돌아오게 한다(버프/힐 등 강조 연출용).
	public void FlashTint(Color color, float duration)
	{
		_tintFlashColor = color;
		_tintFlashDuration = Mathf.Max(duration, 0.01f);
		_tintFlashElapsed = 0f;
		_tintFlashing = true;
	}

	private void UpdateTintFlash(double delta)
	{
		if (!_tintFlashing)
			return;

		_tintFlashElapsed += (float)delta;
		float t = Mathf.Clamp(_tintFlashElapsed / _tintFlashDuration, 0f, 1f);

		if (AnimatedSprite != null)
			AnimatedSprite.Modulate = _tintFlashColor.Lerp(Colors.White, t);

		if (t >= 1f)
			_tintFlashing = false;
	}

	// 주사위 스프라이트를 지정한 색으로 흰색↔색상을 오가며 계속 깜빡인다(버프 지속시간 내내 켜두는 용도). StopTintBlink로 끈다.
	public void StartTintBlink(Color color, float speed)
	{
		_tintBlinking = true;
		_tintBlinkColor = color;
		_tintBlinkSpeed = speed;
		_tintBlinkElapsed = 0f;
	}

	public void StopTintBlink()
	{
		_tintBlinking = false;
		if (DiceSprite != null)
			DiceSprite.Modulate = Colors.White;
	}

	private void UpdateTintBlink(double delta)
	{
		if (!_tintBlinking)
			return;

		_tintBlinkElapsed += (float)delta;
		float wave = (Mathf.Sin(_tintBlinkElapsed * _tintBlinkSpeed) + 1f) * 0.5f;   // 0..1 왕복

		if (DiceSprite != null)
			DiceSprite.Modulate = Colors.White.Lerp(_tintBlinkColor, wave);
	}

	// 보스 몸통을 지정한 색으로 고정 틴트한다(약해짐 등 지속 상태 표시용). ResetBodyTint로 되돌린다.
	public void SetBodyTint(Color color)
	{
		if (AnimatedSprite != null)
			AnimatedSprite.Modulate = color;
	}

	public void ResetBodyTint()
	{
		if (AnimatedSprite != null)
			AnimatedSprite.Modulate = Colors.White;
	}

	private void SpawnGroundZones()
	{
		if (_bossData.MagicCircleScene == null)
		{
			GD.PrintErr($"[Boss] {Name}: MagicCircleScene이 비어있습니다.");
			return;
		}

		Vector2 center = CorePosition;

		int total = _bossData.GroundZoneCount;
		int nearCount = Mathf.Clamp(_bossData.GroundZoneNearCount, 0, total);
		int farCount = total - nearCount;

		// 발밑 가까이 한 무리 + 비교적 멀리 한 무리로 나눠서 배치(각 무리별로 각도 구간을 나눠 겹침 최소화).
		var placed = new Vector2[total];
		int placedCount = 0;
		placedCount = PlaceZoneBand(center, nearCount, 0f, _bossData.GroundZoneNearRange, placed, placedCount);
		placedCount = PlaceZoneBand(center, farCount, _bossData.GroundZoneFarInner, _bossData.GroundZoneSpreadRange, placed, placedCount);

		GD.Print($"[GroundZone] 장판 근접 {nearCount} + 원거리 {farCount}개 소환");
	}

	// center 기준 [minDist, maxDist] 거리 링 안에 count개를 각도 구간(부채꼴)으로 나눠 배치·소환한다.
	// placed에 이미 놓인 장판들과 GroundZoneMinSpacing 이상 떨어지도록 재추첨한다. 반환값은 갱신된 placedCount.
	private int PlaceZoneBand(Vector2 center, int count, float minDist, float maxDist, Vector2[] placed, int placedCount)
	{
		if (count <= 0)
			return placedCount;

		float sectorDegrees = 360f / count;
		const int maxAttemptsPerZone = 30;

		for (int i = 0; i < count; i++)
		{
			Vector2 position = Vector2.Zero;
			for (int attempt = 0; attempt < maxAttemptsPerZone; attempt++)
			{
				float angleDeg = i * sectorDegrees + (float)GD.RandRange(-sectorDegrees * 0.4, sectorDegrees * 0.4);
				float distance = (float)GD.RandRange(minDist, maxDist);
				position = center + Vector2.Right.Rotated(Mathf.DegToRad(angleDeg)) * distance;

				bool tooClose = false;
				for (int j = 0; j < placedCount; j++)
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

			placed[placedCount++] = position;

			MagicCircle circle = _bossData.MagicCircleScene.Instantiate<MagicCircle>();
			Node parent = Blackboard.Main != null ? (Node)Blackboard.EntityContainer : GetParent();
			parent.AddChild(circle);
			circle.GlobalPosition = position;
			circle.Setup(_bossData.GroundZoneRadius, _bossData.GroundZoneTelegraphDuration, _bossData.GroundZoneActiveDuration, _bossData.GroundZoneDamage, Team, TargetNode);
		}

		return placedCount;
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

	// 평상시 위아래로 천천히 둥실거리는 아이들 모션. 매 프레임 위치를 base+float로 다시 잡아준다.
	private void UpdateDiceFloat(double delta)
	{
		if (DiceSprite == null || _bossData == null)
			return;

		_diceFloatElapsed += (float)delta;
		float offsetY = Mathf.Sin(_diceFloatElapsed * _bossData.DiceFloatSpeed) * _bossData.DiceFloatAmplitude;
		DiceSprite.Position = _diceBasePosition + new Vector2(0f, offsetY);
	}

	// 굴리는 동안(또는 버프 지속 중) 주사위 스프라이트를 지금 위치(둥실거림 포함) 기준으로 추가 흔든다.
	// scaleMultiplier를 주면 원래 크기 대비 이만큼 확대(1이면 그대로)도 같이 적용한다.
	public void ShakeDice(float magnitude, float scaleMultiplier = 1f)
	{
		if (DiceSprite == null)
			return;

		DiceSprite.Position += new Vector2(
			(float)GD.RandRange(-magnitude, magnitude),
			(float)GD.RandRange(-magnitude, magnitude));
		DiceSprite.Scale = _diceBaseScale * scaleMultiplier;
	}

	// 굴림/버프가 끝나면 주사위 크기를 원래대로 되돌린다(위치는 둥실거림이 계속 알아서 갱신함).
	public void ResetDicePosition()
	{
		if (DiceSprite != null)
			DiceSprite.Scale = _diceBaseScale;
	}

	// 스포너와 동일한 순서(Data → SetTarget → AddChild)로 방패병 한 마리를 즉석 소환한다.
	public void SummonShield(Vector2 worldPosition)
	{
		if (_bossData?.MonsterScene == null || _bossData.ShieldData == null)
		{
			GD.PrintErr($"[Boss] {Name}: MonsterScene/ShieldData가 비어있습니다.");
			return;
		}

		Monster shield = _bossData.MonsterScene.Instantiate<Monster>();
		shield.Data = _bossData.ShieldData;
		if (TargetNode != null && IsInstanceValid(TargetNode) && Core != null)
			shield.SetTarget(TargetNode.GlobalPosition, Core);

		// TODO(M6 통합 시 롤백): GetParent() 폴백은 독립 테스트 씬 디버깅용 임시 조치.
		// Blackboard.EntityContainer는 내부적으로 Main.BattleWorldHud를 타는데, Main이 없는
		// 독립 씬에서는 그 자체가 NullReferenceException을 던져서 임시로 우회해둔 것.
		// 실제 battle_world 통합 후엔 Blackboard.EntityContainer만 쓰도록 되돌릴 것.
		Node parent = Blackboard.Main != null ? (Node)Blackboard.EntityContainer : GetParent();
		parent.AddChild(shield);
		shield.GlobalPosition = worldPosition;
		shield.Scale = Vector2.One * (_bossData?.ShieldScale ?? 1f);
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
		LimitPlayerMovement();
		_bossAnimator = new BossAnimator(AnimatedSprite);
		_patterns = new BossPatternController(this);
	}

	// 보스 웨이브 진입 시 플레이어의 오른쪽 이동 한계를 보스 왼쪽 PlayerMoveLimitOffset 지점으로 잡는다(위아래 무제한).
	private void LimitPlayerMovement()
	{
		if (_bossData == null)
			return;

		if (GetTree().GetFirstNodeInGroup("player") is Player player)
			player.SetRightLimit(GlobalPosition.X - _bossData.PlayerMoveLimitOffset);
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
		UpdateDiceFloat(delta);
		_patterns?.Tick(delta);
		UpdateGroundZoneAttack(delta);
		UpdateTintFlash(delta);
		UpdateTintBlink(delta);
	}

	// 스턴 상태이상 재해석: debuff 애니만 재생하고, 주사위 굴림·시전은 스턴과 무관하게 계속 진행한다.
	protected override void OnStunned(float delta)
	{
		_bossAnimator?.PlayDebuff();
		UpdateDiceFloat(delta);
		_patterns?.Tick(delta);
		UpdateGroundZoneAttack(delta);
		UpdateTintFlash(delta);
		UpdateTintBlink(delta);
	}

	// autoplay로 SpriteFrames가 자체 재생하는 걸 막고, SetDiceFace로만 프레임을 제어한다.
	private void SetupDiceSprite()
	{
		if (DiceSprite == null)
			return;

		_diceBasePosition = DiceSprite.Position;
		_diceBaseScale = DiceSprite.Scale;
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
