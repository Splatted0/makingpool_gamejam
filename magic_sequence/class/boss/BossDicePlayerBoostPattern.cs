// 주사위 7(조커): 플레이어 공격속도를 PlayerAttackSpeedMultiplier배로 PlayerBoostDuration초 동안 버프.
// WandManager.FireCooldown(발사 간격의 기준값)을 낮췄다가 시간 다 되면 원래대로 복원한다.
// 진행 중인 발사 시퀀스도 슬롯 딜레이가 매 발사마다 새로 계산되므로 즉시 반영된다.
public class BossDicePlayerBoostPattern : IBossPattern
{
	private float _elapsed;
	private double _originalFireCooldown;
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
		BossData data = boss.Config;
		_originalFireCooldown = _wandManager.FireCooldown;
		_wandManager.FireCooldown = _originalFireCooldown / Mathf.Max(data.PlayerAttackSpeedMultiplier, 0.01f);

		_elapsed = 0f;
		_finished = false;
		GD.Print($"[Dice7] 공속 {data.PlayerAttackSpeedMultiplier}배 버프 시작");
	}

	public void Tick(Boss boss, double delta)
	{
		if (_finished)
			return;

		_elapsed += (float)delta;
		if (_elapsed >= boss.Config.PlayerBoostDuration)
		{
			_wandManager.FireCooldown = _originalFireCooldown;
			GD.Print("[Dice7] 공속 버프 종료");
			_finished = true;
		}
	}
}
