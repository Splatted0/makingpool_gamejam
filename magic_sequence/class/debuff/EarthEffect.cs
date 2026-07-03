// 스턴(Earth). 지속시간 동안 몬스터 이동·공격 정지. 스택은 사용 안 함(효과 동일).
public class EarthEffect : IDebuff
{
	public float Duration { get; set; } = 1.5f;
	public int Stacks { get; set; } = 1;

	public void OnApply(Monster monster)
	{
		monster.IsStunned = true;
	}

	public void OnTick(Monster monster, float delta)
	{
	}

	public void OnExpire(Monster monster)
	{
		monster.IsStunned = false;
	}
}
