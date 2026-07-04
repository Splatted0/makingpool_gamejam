// 주사위 6: 왼쪽(-X) 기준 ±(LaserSprayArcDegrees/2) 안에서 방향을 무작위로 뽑아
// 한 발씩 순차로 쏜다(6발 동시 아님). 발마다 짧게 차징(가늘게→굵게)한 뒤 판정+발사.
// 코어를 조준하지 않으므로, 발사 순간 코어가 그 직선 근처에 있으면 맞고 아니면 빗나간다.
public class BossDiceLaserSprayPattern : IBossPattern
{
	private const float HitRadius = 24f;
	private const float ChargeWidth = 1f;
	private const float FireWidth = 6f;

	private int _shotsFired;
	private float _chargeElapsed;
	private bool _charging;
	private bool _finished = true;
	private Vector2 _currentDirection;
	private BossLaserSprayBeam _currentBeam;

	public bool IsFinished => _finished;
	public bool WasCancelled => false;

	public void Start(Boss boss)
	{
		_shotsFired = 0;
		_finished = false;
		BeginNextShot(boss);
	}

	public void Tick(Boss boss, double delta)
	{
		if (_finished || !_charging)
			return;

		BossData data = boss.Config;
		_chargeElapsed += (float)delta;
		float t = Mathf.Clamp(_chargeElapsed / data.LaserSprayChargeTime, 0f, 1f);
		_currentBeam?.SetWidth(Mathf.Lerp(ChargeWidth, FireWidth, t));

		if (_chargeElapsed >= data.LaserSprayChargeTime)
			FireCurrentShot(boss);
	}

	private void BeginNextShot(Boss boss)
	{
		BossData data = boss.Config;
		float halfArc = data.LaserSprayArcDegrees * 0.5f;
		float angleDeg = (float)GD.RandRange(-halfArc, halfArc);
		_currentDirection = Vector2.Left.Rotated(Mathf.DegToRad(angleDeg));

		_currentBeam = new BossLaserSprayBeam();
		boss.GetParent().AddChild(_currentBeam);
		_currentBeam.GlobalPosition = boss.GlobalPosition;
		_currentBeam.Setup(_currentDirection, data.LaserSprayLength, new Color(1f, 0.15f, 0.25f, 0.85f), ChargeWidth);

		_chargeElapsed = 0f;
		_charging = true;
	}

	private void FireCurrentShot(Boss boss)
	{
		BossData data = boss.Config;
		_charging = false;

		if (HitsCore(boss, _currentDirection, data.LaserSprayLength))
			boss.HitCore(data.LaserSprayDamage);

		_currentBeam?.SetWidth(FireWidth);
		_currentBeam?.Fire(data.LaserSprayFadeDuration);
		_currentBeam = null;
		boss.PlayAttackAnim();

		_shotsFired++;
		int total = Mathf.Max(data.LaserSprayCount, 1);
		GD.Print($"[Dice6] {_shotsFired}/{total}");

		if (_shotsFired >= total)
			_finished = true;
		else
			BeginNextShot(boss);
	}

	// 콜리전 아님 — 점(코어)과 선분(레이저 경로) 사이 최단거리를 계산하는 순수 기하 판정.
	private bool HitsCore(Boss boss, Vector2 direction, float length)
	{
		Vector2 start = boss.GlobalPosition;
		Vector2 segment = direction * length;
		float segmentLengthSq = segment.LengthSquared();

		float t = segmentLengthSq > 0f
			? Mathf.Clamp((boss.CorePosition - start).Dot(segment) / segmentLengthSq, 0f, 1f)
			: 0f;

		Vector2 closest = start + segment * t;
		return closest.DistanceTo(boss.CorePosition) <= HitRadius;
	}
}
