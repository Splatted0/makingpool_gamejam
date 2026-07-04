// 주사위 7(조커): WandManager의 재발사 대기시간(GetBaseCooldown)에 PlayerCooldownMultiplier를
// 곱해 거의 없앤 채로 PlayerRapidFireDuration초 동안 유지한다 — 연타하면 그대로 연사됨.
// 시간이 다 되면 배율을 1.0으로 복원한다.
public class BossDicePlayerBoostPattern : IBossPattern
{
	private float _elapsed;
	private WandManager _wandManager;
	private bool _finished = true;
	private bool _cancelled;

	public bool IsFinished => _finished;
	public bool WasCancelled => _cancelled;

	public void Start(Boss boss)
	{
		boss.PlayLuckyAnim();

		if (Blackboard.Main == null)
		{
			GD.Print("[Dice7] 독립 씬이라 스킵");
			_cancelled = true;
			_finished = true;
			return;
		}

		_wandManager = Blackboard.BattleWorldHud?.GetNodeOrNull<WandManager>("BattleCenter/WandManager");
		if (_wandManager == null)
		{
			GD.Print("[Dice7] WandManager 없음, 스킵");
			_cancelled = true;
			_finished = true;
			return;
		}

		_cancelled = false;
		_wandManager.SetCooldownMultiplier(boss.Config.PlayerCooldownMultiplier);

		_elapsed = 0f;
		_finished = false;
		GD.Print("[Dice7] 연사 조커 시작");
	}

	public void Tick(Boss boss, double delta)
	{
		if (_finished)
			return;

		_elapsed += (float)delta;
		if (_elapsed >= boss.Config.PlayerRapidFireDuration)
		{
			_wandManager.SetCooldownMultiplier(1.0);
			GD.Print("[Dice7] 연사 조커 종료");
			_finished = true;
		}
	}
}
