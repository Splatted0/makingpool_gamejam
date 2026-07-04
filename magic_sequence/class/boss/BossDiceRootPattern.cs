// 주사위 4: 코어를 CoreRootDuration초 동안 속박(목줄 무시, 제자리 고정)해 회피 이동을 무력화한다.
public class BossDiceRootPattern : IBossPattern
{
	private float _elapsed;
	private bool _finished = true;

	public bool IsFinished => _finished;

	public void Start(Boss boss)
	{
		_elapsed = 0f;
		_finished = false;
		boss.SetCoreRooted(true);
		boss.PlayDebuffAnim();
		GD.Print("[Dice4] 코어 속박");
	}

	public void Tick(Boss boss, double delta)
	{
		if (_finished)
			return;

		_elapsed += (float)delta;
		if (_elapsed >= boss.Config.CoreRootDuration)
		{
			boss.SetCoreRooted(false);
			GD.Print("[Dice4] 속박 해제");
			_finished = true;
		}
	}
}
