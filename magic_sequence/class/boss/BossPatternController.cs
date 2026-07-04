// 보스 패턴 스케줄러. "언제 어떤 패턴을 돌릴지"만 결정하고, 실제 동작은 IBossPattern 조각에 위임한다.
// 레이저·탄막을 각자 주기로 돌린다. M5에서 3초 주사위 방해 패턴이 여기에 얹힌다.
public class BossPatternController
{
	private readonly Boss _boss;
	private readonly BossLaserPattern _laser = new BossLaserPattern();   // 스턴 캔슬 위해 구체 타입
	private readonly IBossPattern _barrage = new BossBarragePattern();
	private readonly BossDicePattern _dice = new BossDicePattern();
	private IBossPattern _activeDiceEffect;   // 굴림 끝나고 확정된 얼굴에 대응하는 방해 패턴
	private double _laserCooldown;
	private double _barrageCooldown;
	private double _diceCooldown;

	public BossPatternController(Boss boss)
	{
		_boss = boss;
		_laserCooldown = boss.Config != null ? boss.Config.LaserInterval : 0;
		_barrageCooldown = boss.Config != null ? boss.Config.BarrageInterval : 0;
		_diceCooldown = boss.Config != null ? boss.Config.DiceInterval : 0;
	}

	public void Tick(double delta)
	{
		if (_boss.Config == null)
			return;

		TickLaser(delta);
		TickBarrage(delta);
		TickDice(delta);
	}

	// 스턴 중 호출(매 프레임). 충전 중인 레이저를 캔슬하고 쿨다운을 새로 잡는다.
	public void OnStunned()
	{
		if (_boss.Config == null)
			return;

		if (!_laser.IsFinished)
			GD.Print("[Laser] 캔슬(스턴)");

		_laser.Cancel();
		_boss.SetBeamWidth(_boss.Config.LaserWidthIdle);
		_laserCooldown = _boss.Config.LaserInterval;   // 스턴 풀리면 새 주기부터 다시 충전
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

	// 주사위를 주기(DiceInterval)마다 굴림. 진행 중이면 tick, 끝나면 결과 패턴을 디스패치.
	// 디스패치된 패턴이 끝날 때까지 다음 롤은 시작하지 않는다(주사위끼리만 캐스트락).
	private void TickDice(double delta)
	{
		if (_activeDiceEffect != null)
		{
			_activeDiceEffect.Tick(_boss, delta);
			if (_activeDiceEffect.IsFinished)
			{
				_activeDiceEffect = null;
				_diceCooldown = _boss.Config.DiceInterval;
			}
			return;
		}

		if (_dice.IsFinished)
		{
			_diceCooldown -= delta;
			if (_diceCooldown <= 0)
				_dice.Start(_boss);
			return;
		}

		_dice.Tick(_boss, delta);
		if (_dice.IsFinished)
			DispatchDiceResult();
	}

	private void DispatchDiceResult()
	{
		switch (_dice.ResultFace)
		{
			case 1:
				_activeDiceEffect = new BossDiceShieldPattern();
				break;
			case 3:
				_activeDiceEffect = new BossDiceBarragePattern();
				break;
			case 6:
				_activeDiceEffect = new BossDiceLaserSprayPattern();
				break;
			default:
				GD.Print($"[Dice] {_dice.ResultFace} 미구현");
				_diceCooldown = _boss.Config.DiceInterval;
				return;
		}

		_activeDiceEffect.Start(_boss);
	}
}
