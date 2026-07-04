// 탄막 1사이클: 코어 방향 기준 부채꼴로 탄을 동시에 발사하고 즉시 종료(예고 없이 순간 발사).
public class BossBarragePattern : IBossPattern
{
	private bool _finished = true;

	public bool IsFinished => _finished;

	public void Start(Boss boss)
	{
		BossData data = boss.Config;

		Vector2 baseDirection = (boss.CorePosition - boss.GlobalPosition).Normalized();
		if (baseDirection == Vector2.Zero)
			baseDirection = Vector2.Left;

		int count = Mathf.Max(data.BarrageBulletCount, 1);
		float halfSpread = data.BarrageSpreadDegrees * 0.5f;

		// 슬로우(Ice) 상태이상 재해석: MoveSpeedMultiplier(슬로우 시 <1)를 탄속에 곱해 탄막을 느리게.
		float speed = data.BarrageBulletSpeed * boss.MoveSpeedMultiplier;

		for (int i = 0; i < count; i++)
		{
			float t = count == 1 ? 0.5f : (float)i / (count - 1);
			float angleDeg = Mathf.Lerp(-halfSpread, halfSpread, t);
			Vector2 direction = baseDirection.Rotated(Mathf.DegToRad(angleDeg));
			boss.SpawnBullet(direction, speed, data.BarrageDamage);
		}

		GD.Print($"[Barrage] 발사 {count}발 (탄속 {speed:F0})");
		_finished = true;
	}

	public void Tick(Boss boss, double delta)
	{
	}
}
