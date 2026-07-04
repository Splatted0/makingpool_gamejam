// 주사위 5: 자기 회복. 최소 1, 최대는 현재 잃은 체력만큼 랜덤량으로 즉시 회복한다.
public class BossDiceHealPattern : IBossPattern
{
	private bool _finished = true;

	public bool IsFinished => _finished;

	public void Start(Boss boss)
	{
		boss.PlayBuffAnim();

		int missing = boss.Data.MaxHealth - boss.Health;
		if (missing <= 0)
		{
			GD.Print("[Dice5] 이미 풀피, 회복량 없음");
			_finished = true;
			return;
		}

		int amount = GD.RandRange(1, missing);
		boss.Heal(amount);
		GD.Print($"[Dice5] 자힐 {amount} (Health {boss.Health}/{boss.Data.MaxHealth})");
		_finished = true;
	}

	public void Tick(Boss boss, double delta)
	{
	}
}
