// 주사위 7(조커): WandManager의 재발사 대기시간(GetBaseCooldown)에 PlayerCooldownMultiplier를
// 곱해 거의 없앤 채로 PlayerRapidFireDuration초 동안 유지한다 — 연타하면 그대로 연사됨.
// 연출(주사위 초록 명멸+확대·흔들림, 보스 몸통은 살짝 어두운 틴트로 "약해짐" 표시)은 게임
// 컨텍스트 유무와 무관하게 항상 재생되고, 실제 쿨다운 배율 적용은 WandManager를 찾을 수 있을 때만 처리한다.
public class BossDicePlayerBoostPattern : IBossPattern
{
	private float _elapsed;
	private WandManager _wandManager;
	private bool _finished = true;

	public bool IsFinished => _finished;
	public bool WasCancelled => false;

	public void Start(Boss boss)
	{
		boss.PlayLuckyAnim();
		boss.StartTintBlink(boss.Config.PlayerBoostTintColor, boss.Config.PlayerBoostBlinkSpeed);
		boss.SetBodyTint(boss.Config.PlayerBoostBodyTintColor);   // 몸통은 살짝 어둡게(약해짐 표시)

		_elapsed = 0f;
		_finished = false;
		_wandManager = null;

		if (Blackboard.Main == null)
		{
			GD.Print("[Dice7] 독립 씬이라 연사 효과는 스킵(연출만 재생)");
			return;
		}

		_wandManager = Blackboard.BattleWorldHud?.GetNodeOrNull<WandManager>("BattleCenter/WandManager");
		if (_wandManager == null)
		{
			GD.Print("[Dice7] WandManager 없음(연출만 재생)");
			return;
		}

		_wandManager.SetCooldownMultiplier(boss.Config.PlayerCooldownMultiplier);
		GD.Print("[Dice7] 연사 조커 시작");
	}

	// 보스가 버프 도중 죽으면 쿨다운 배율(0.02)이 영구 지속되지 않게 즉시 원복한다.
	public void Cancel(Boss boss)
	{
		if (_finished)
			return;

		_wandManager?.SetCooldownMultiplier(1.0);
		boss.StopTintBlink();
		boss.ResetBodyTint();
		boss.ResetDicePosition();
		_finished = true;
	}

	public void Tick(Boss boss, double delta)
	{
		if (_finished)
			return;

		boss.ShakeDice(boss.Config.PlayerBoostDiceShakeMagnitude, boss.Config.PlayerBoostDiceScale);

		_elapsed += (float)delta;
		if (_elapsed >= boss.Config.PlayerRapidFireDuration)
		{
			_wandManager?.SetCooldownMultiplier(1.0);
			boss.StopTintBlink();
			boss.ResetBodyTint();
			boss.ResetDicePosition();
			GD.Print("[Dice7] 연사 조커 종료");
			_finished = true;
		}
	}
}
