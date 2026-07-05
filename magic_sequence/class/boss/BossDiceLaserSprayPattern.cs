using System.Collections.Generic;

// 주사위 6: 왼쪽(-X) 기준 ±(LaserSprayArcDegrees/2) 안에서 방향을 무작위로 뽑아 여러 발을 쏜다.
// 얇은 선행 레이저가 LaserSprayShotInterval 간격으로 다라락 연속으로 나가고, 각 발은 자기가
// 나온 시점 기준 LaserSprayChargeTime 뒤에 독립적으로 굵은 임팩트가 터진다(발끼리 겹쳐 진행됨).
// 코어를 조준하지 않으므로, 발사 순간 코어가 그 직선 근처에 있으면 맞고 아니면 빗나간다.
public class BossDiceLaserSprayPattern : IBossPattern
{
	private class ActiveShot
	{
		public BossLaserSprayBeam Beam;
		public Vector2 Direction;
		public float ChargeElapsed;
	}

	private readonly List<ActiveShot> _activeShots = new List<ActiveShot>();
	private int _shotsSpawned;
	private float _spawnElapsed;
	private bool _finished = true;

	public bool IsFinished => _finished;
	public bool WasCancelled => false;

	public void Start(Boss boss)
	{
		_activeShots.Clear();
		_shotsSpawned = 0;
		_spawnElapsed = 0f;
		_finished = false;

		SpawnShot(boss);
	}

	public void Tick(Boss boss, double delta)
	{
		if (_finished)
			return;

		BossData data = boss.Config;
		int total = Mathf.Max(data.LaserSprayCount, 1);

		if (_shotsSpawned < total)
		{
			_spawnElapsed += (float)delta;
			if (_spawnElapsed >= data.LaserSprayShotInterval)
			{
				_spawnElapsed = 0f;
				SpawnShot(boss);
			}
		}

		for (int i = _activeShots.Count - 1; i >= 0; i--)
		{
			ActiveShot shot = _activeShots[i];
			shot.ChargeElapsed += (float)delta;
			if (shot.ChargeElapsed >= data.LaserSprayChargeTime)
			{
				FireShot(boss, shot);
				_activeShots.RemoveAt(i);
			}
		}

		if (_shotsSpawned >= total && _activeShots.Count == 0)
			_finished = true;
	}

	// 보스가 발사 도중 죽으면 아직 임팩트가 안 터진 예고선들이 화면에 영원히 남지 않게 페이드아웃시킨다.
	public void Cancel(Boss boss)
	{
		if (_finished)
			return;

		foreach (ActiveShot shot in _activeShots)
			shot.Beam?.Fire(boss.Config.LaserSprayFadeDuration);   // 판정 없이 페이드아웃만
		_activeShots.Clear();
		_finished = true;
	}

	private void SpawnShot(Boss boss)
	{
		BossData data = boss.Config;
		float halfArc = data.LaserSprayArcDegrees * 0.5f;
		float angleDeg = (float)GD.RandRange(-halfArc, halfArc);
		Vector2 direction = Vector2.Left.Rotated(Mathf.DegToRad(angleDeg));

		BossLaserSprayBeam beam = new BossLaserSprayBeam();
		boss.GetParent().AddChild(beam);
		beam.GlobalPosition = GetOrigin(boss);
		beam.Setup(direction, data.LaserSprayLength, data.LaserSprayBackExtension, data.LaserTexture, data.LaserSprayThinWidth);

		_activeShots.Add(new ActiveShot { Beam = beam, Direction = direction, ChargeElapsed = 0f });
		_shotsSpawned++;
	}

	private void FireShot(Boss boss, ActiveShot shot)
	{
		BossData data = boss.Config;

		if (HitsCore(boss, shot.Direction, data.LaserSprayLength))
			boss.HitCore(data.LaserSprayDamage);

		// 얇은 선행 레이저 → 판정 순간 굵은 임팩트 레이저로 스냅(보간 아님, 순간 전환)
		shot.Beam.SetWidth(data.LaserSprayImpactWidth);
		shot.Beam.Fire(data.LaserSprayFadeDuration);
		boss.PlayAttackAnim();

		GD.Print($"[Dice6] {_shotsSpawned}/{Mathf.Max(data.LaserSprayCount, 1)} 임팩트");
	}

	// 보스 기준 오른쪽으로 물러난 실제 발사 시작점(몸통을 뚫고 나오지 않게).
	private static Vector2 GetOrigin(Boss boss) => boss.GlobalPosition + new Vector2(boss.Config.LaserSpawnOffsetX, 0f);

	// 콜리전 아님 — 점(코어)과 선분(레이저 경로) 사이 최단거리를 계산하는 순수 기하 판정.
	private bool HitsCore(Boss boss, Vector2 direction, float length)
	{
		Vector2 start = GetOrigin(boss);
		Vector2 segment = direction * length;
		float segmentLengthSq = segment.LengthSquared();

		float t = segmentLengthSq > 0f
			? Mathf.Clamp((boss.CorePosition - start).Dot(segment) / segmentLengthSq, 0f, 1f)
			: 0f;

		Vector2 closest = start + segment * t;
		return closest.DistanceTo(boss.CorePosition) <= boss.Config.LaserSprayHitRadius;
	}
}
