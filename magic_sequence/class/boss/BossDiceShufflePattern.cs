// 주사위 2: 지팡이 3개 각각의 마법 슬롯 순서를 피셔-예이츠로 섞는다.
// Wand.Magics 변경 자체는 WandNode.Active()가 매번 라이브로 읽어서 즉시 반영되지만,
// 무기고 UI(Arsenal)는 WandManager.SetupWands()를 불러야 갱신되므로 셔플 후 반드시 호출한다.
public class BossDiceShufflePattern : IBossPattern
{
	private bool _finished = true;
	private bool _cancelled;

	public bool IsFinished => _finished;
	public bool WasCancelled => _cancelled;

	public void Start(Boss boss)
	{
		// 독립 씬에서도 연출(애니+틴트)은 보이게, 실제 완드 셔플 로직만 게임 컨텍스트가 있을 때 처리한다.
		boss.PlayDebuffAnim();
		boss.FlashTint(new Color(2.2f, 0.35f, 0.3f, 1f), 0.5f);   // 디버프 강조: 쨍한 빨강 틴트

		if (boss.GetTree().GetFirstNodeInGroup("player") is Player player)
			player.FlashTint(new Color(2.2f, 0.35f, 0.3f, 1f), 0.5f);   // 플레이어도 디버프 맞았다는 걸 빨강으로 표시

		if (Blackboard.Main == null)
		{
			GD.Print("[Dice2] 독립 씬이라 셔플은 스킵");
			_cancelled = true;
			_finished = true;
			return;
		}

		_cancelled = false;

		Wand[] wands = Blackboard.Wands;
		if (wands != null)
			foreach (Wand wand in wands)
				ShuffleWand(wand);

		WandManager wandManager = Blackboard.BattleWorldHud?.GetNodeOrNull<WandManager>("BattleCenter/WandManager");
		wandManager?.SetupWands();

		GD.Print("[Dice2] 지팡이 셔플");
		_finished = true;
	}

	public void Tick(Boss boss, double delta)
	{
	}

	private static void ShuffleWand(Wand wand)
	{
		if (wand?.Magics == null)
			return;

		int count = wand.Magics.Count;
		for (int i = count - 1; i > 0; i--)
		{
			int j = GD.RandRange(0, i);
			wand.Swap(i, j);
		}
	}
}
