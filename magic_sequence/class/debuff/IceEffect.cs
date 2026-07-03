// 슬로우(Ice). 지속시간 동안 몬스터 이동속도 감소. 스택이 오를수록 감속량 증가.
public class IceEffect : IDebuff
{
	private const float BASE_SLOW = 0.5f;   // 1스택 기준 감속량(0.5 = 속도 절반)

	public float Duration { get; set; } = 3f;
	public int Stacks { get; set; } = 1;

	public void OnApply(Monster monster)
	{
		float slow = BASE_SLOW * StackMultiplier();
		monster.MoveSpeedMultiplier = Mathf.Max(1f - slow, 0f);
	}

	public void OnTick(Monster monster, float delta)
	{
	}

	public void OnExpire(Monster monster)
	{
		monster.MoveSpeedMultiplier = 1f;
	}

	// 1스택 1배, 2스택 1.1배, 3스택 1.2배
	private float StackMultiplier()
	{
		return 1f + (Stacks - 1) * 0.1f;
	}
}
