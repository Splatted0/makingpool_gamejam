// 보스 패턴 스케줄러. 탄막·레이저는 상시 자동 패턴이 아니라 주사위(3/6) 결과로만 나간다.
// 여기서는 주사위 롤 스케줄링과 결과 디스패치만 담당한다.
public class BossPatternController
{
	private readonly Boss _boss;
	private readonly BossDicePattern _dice = new BossDicePattern();
	private IBossPattern _activeDiceEffect;   // 굴림 끝나고 확정된 얼굴에 대응하는 방해 패턴
	private double _diceCooldown;

	public BossPatternController(Boss boss)
	{
		_boss = boss;
		_diceCooldown = boss.Config != null ? boss.Config.DiceInterval : 0;
	}

	public void Tick(double delta)
	{
		if (_boss.Config == null)
			return;

		TickDice(delta);
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
				bool cancelled = _activeDiceEffect.WasCancelled;
				_activeDiceEffect = null;
				// 캔슬(조건 미충족으로 아무 효과 없음)이면 쿨다운 없이 바로 다음 주사위로 넘어간다.
				_diceCooldown = cancelled ? 0 : _boss.Config.DiceInterval;
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
			case 2:
				_activeDiceEffect = new BossDiceShufflePattern();
				break;
			case 3:
				_activeDiceEffect = new BossDiceBarragePattern();
				break;
			case 4:
				_activeDiceEffect = new BossDiceRootPattern();
				break;
			case 5:
				_activeDiceEffect = new BossDiceHealPattern();
				break;
			case 6:
				_activeDiceEffect = new BossDiceLaserSprayPattern();
				break;
			case 7:
				_activeDiceEffect = new BossDicePlayerBoostPattern();
				break;
			default:
				GD.Print($"[Dice] {_dice.ResultFace} 미구현");
				_diceCooldown = _boss.Config.DiceInterval;
				return;
		}

		_activeDiceEffect.Start(_boss);
	}
}
