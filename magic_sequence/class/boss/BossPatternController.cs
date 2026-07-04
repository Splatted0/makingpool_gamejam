// 보스 패턴 스케줄러. "언제 어떤 패턴을 돌릴지"만 결정하고, 실제 동작은 IBossPattern 조각에 위임한다.
// 레이저·탄막을 각자 주기로 돌린다. M5에서 3초 주사위 방해 패턴이 여기에 얹힌다.
public class BossPatternController
{
	private readonly Boss _boss;
	private readonly IBossPattern _laser = new BossLaserPattern();
	private readonly IBossPattern _barrage = new BossBarragePattern();
	private double _laserCooldown;
	private double _barrageCooldown;

	public BossPatternController(Boss boss)
	{
		_boss = boss;
		_laserCooldown = boss.Config != null ? boss.Config.LaserInterval : 0;
		_barrageCooldown = boss.Config != null ? boss.Config.BarrageInterval : 0;
	}

	public void Tick(double delta)
	{
		if (_boss.Config == null)
			return;

		TickLaser(delta);
		TickBarrage(delta);
	}

	// 레이저를 주기(LaserInterval)마다 재실행. 진행 중이면 tick, 끝나면 쿨다운 후 재시작.
	private void TickLaser(double delta)
	{
		if (_laser.IsFinished)
		{
			_boss.SetBeamWidth(_boss.Config.LaserWidthIdle);
			_laserCooldown -= delta;
			if (_laserCooldown <= 0)
				_laser.Start(_boss);
			return;
		}

		_laser.Tick(_boss, delta);
		if (_laser.IsFinished)
		{
			_laserCooldown = _boss.Config.LaserInterval;
			GD.Print("[Laser] 재사용 대기");
		}
	}

	// 탄막은 Start() 안에서 즉시 발사+종료되는 패턴이라, 여기서 바로 쿨다운을 리셋해야
	// 다음 프레임에 또 발사되는 걸 막는다(레이저처럼 진행 중 상태가 없어 Tick 분기를 못 탐).
	private void TickBarrage(double delta)
	{
		if (_barrage.IsFinished)
		{
			_barrageCooldown -= delta;
			if (_barrageCooldown <= 0)
			{
				_barrage.Start(_boss);
				_barrageCooldown = _boss.Config.BarrageInterval;
				GD.Print("[Barrage] 재사용 대기");
			}
			return;
		}

		_barrage.Tick(_boss, delta);
	}
}
