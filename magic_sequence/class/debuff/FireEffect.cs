// 화상(Fire). 지속시간 동안 일정 간격으로 틱데미지. 스택이 오를수록 틱데미지 증가.
public class FireEffect : IDebuff
{
	private const float TICK_INTERVAL = 0.5f;   // 틱 간격(초)
	private const int BASE_TICK_DAMAGE = 1;     // 1스택 기준 틱당 데미지

	private float _tickTimer;

	public float Duration { get; set; } = 3f;
	public int Stacks { get; set; } = 1;

	public void OnApply(Monster monster)
	{
	}

	public void OnTick(Monster monster, float delta)
	{
		_tickTimer += delta;
		if (_tickTimer >= TICK_INTERVAL)
		{
			_tickTimer -= TICK_INTERVAL;
			int damage = Mathf.RoundToInt(BASE_TICK_DAMAGE * StackMultiplier());
			monster.TakeDamage(damage, ColorPreset.Red);   // 화상 데미지는 빨간 팝업
		}
	}

	public void OnExpire(Monster monster)
	{
	}

	// 1스택 1배, 2스택 1.1배, 3스택 1.2배
	private float StackMultiplier()
	{
		return 1f + (Stacks - 1) * 0.1f;
	}
}
