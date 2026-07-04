// 주사위 3: 탄막 부채꼴을 짧은 간격으로 연달아 발사(탕탕탕). 매 연발마다 기준 각도를 살짝 틀어
// 완전히 같은 부채꼴이 반복되지 않게 한다. 부채꼴 자체는 BossBarragePattern.FireFan을 그대로 재사용.
public class BossDiceBarragePattern : IBossPattern
{
	private int _shotsFired;
	private float _gapElapsed;
	private bool _finished = true;

	public bool IsFinished => _finished;

	public void Start(Boss boss)
	{
		_shotsFired = 0;
		_gapElapsed = 0f;
		_finished = false;
		FireVolley(boss);
	}

	public void Tick(Boss boss, double delta)
	{
		if (_finished)
			return;

		BossData data = boss.Config;
		int burstCount = Mathf.Max(data.BarrageBurstCount, 1);
		if (_shotsFired >= burstCount)
		{
			_finished = true;
			return;
		}

		_gapElapsed += (float)delta;
		if (_gapElapsed >= data.BarrageBurstGap)
		{
			_gapElapsed = 0f;
			FireVolley(boss);
		}
	}

	private void FireVolley(Boss boss)
	{
		BossData data = boss.Config;

		Vector2 baseDirection = (boss.CorePosition - boss.GlobalPosition).Normalized();
		if (baseDirection == Vector2.Zero)
			baseDirection = Vector2.Left;

		float jitterDeg = (float)GD.RandRange(-data.BarrageBurstAngleJitter, data.BarrageBurstAngleJitter);
		baseDirection = baseDirection.Rotated(Mathf.DegToRad(jitterDeg));

		BossBarragePattern.FireFan(boss, baseDirection);

		_shotsFired++;
		GD.Print("[Dice3] 발사");
	}
}
