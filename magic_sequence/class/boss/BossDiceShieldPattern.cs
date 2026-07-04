// 주사위 1: 보스 전방(왼쪽)에 방패병을 세로 한 줄로 즉시 소환해 벽을 세운다.
// 방패병 자체는 ShieldData(MoveSpeed=0, 원거리형+데미지0 권장)로 제자리에 가만히 세워두고,
// 마법 타겟팅이 물리 오버랩 순서라 벽으로 세워두는 것만으로 코어행 마법을 몸으로 가로막는다.
public class BossDiceShieldPattern : IBossPattern
{
	private bool _finished = true;

	public bool IsFinished => _finished;

	public void Start(Boss boss)
	{
		BossData data = boss.Config;
		int count = Mathf.Max(data.ShieldSummonCount, 1);
		float totalHeight = (count - 1) * data.ShieldSummonSpacing;
		Vector2 origin = boss.GlobalPosition + new Vector2(-data.ShieldSummonForwardOffset, -totalHeight * 0.5f);

		for (int i = 0; i < count; i++)
		{
			Vector2 position = origin + new Vector2(0f, i * data.ShieldSummonSpacing);
			boss.SummonShield(position);
		}

		GD.Print($"[Dice1] 방패병 {count}마리 소환");
		_finished = true;
	}

	public void Tick(Boss boss, double delta)
	{
	}
}
