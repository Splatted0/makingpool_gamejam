// 레이저 1사이클: 차지 동안 예고선이 굵어지고, 차지가 끝나면 코어에 직격(히트스캔) 후 종료.
// 예고선 자체(보스↔코어 상시 빨간 선)는 Boss가 그리고, 이 패턴은 차지 중 두께만 키우고 발사만 담당.
public class BossLaserPattern : IBossPattern
{
	private float _charge;
	private bool _finished = true;

	public bool IsFinished => _finished;

	public void Start(Boss boss)
	{
		_charge = 0f;
		_finished = false;
		GD.Print("[Laser] 충전");
	}

	public void Tick(Boss boss, double delta)
	{
		if (_finished)
			return;

		BossData data = boss.Config;
		_charge += (float)delta;

		// 차지 진행도에 따라 예고선 두께 보간(얇음 → 굵음)
		float t = Mathf.Clamp(_charge / data.LaserChargeTime, 0f, 1f);
		boss.SetBeamWidth(Mathf.Lerp(data.LaserWidthIdle, data.LaserWidthMax, t));

		if (_charge >= data.LaserChargeTime)
		{
			GD.Print("[Laser] 발사");
			boss.HitCore(data.LaserDamage);   // 코어 직격
			_finished = true;
		}
	}
}
