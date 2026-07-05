// 주사위 5: 자기 회복. 최소 1, 최대는 현재 잃은 체력만큼 랜덤량으로 즉시 회복한다.
// 회복은 즉시 처리하되, 힐 "+" VFX는 짧은 간격으로 여러 개 다라락 띄운다(단발이라 밋밋해서).
public class BossDiceHealPattern : IBossPattern
{
	private bool _finished = true;
	private bool _cancelled;
	private int _vfxThrown;
	private float _vfxElapsed;
	private Vector2 _vfxOrigin;

	public bool IsFinished => _finished;
	public bool WasCancelled => _cancelled;

	public void Start(Boss boss)
	{
		boss.PlayBuffAnim();

		int missing = boss.Data.MaxHealth - boss.Health;
		if (missing <= 0)
		{
			GD.Print("[Dice5] 이미 풀피, 회복량 없음");
			_cancelled = true;
			_finished = true;
			return;
		}

		_cancelled = false;
		int amount = GD.RandRange(1, missing);
		boss.Heal(amount);

		_vfxOrigin = boss.GlobalPosition;
		_vfxThrown = 0;
		_vfxElapsed = boss.Config.HealVfxBurstInterval;   // 첫 개는 첫 Tick에 바로 뜨게
		_finished = false;

		GD.Print($"[Dice5] 자힐 {amount} (Health {boss.Health}/{boss.Data.MaxHealth})");
	}

	public void Tick(Boss boss, double delta)
	{
		if (_finished)
			return;

		_vfxElapsed += (float)delta;
		if (_vfxElapsed < boss.Config.HealVfxBurstInterval)
			return;

		_vfxElapsed = 0f;
		float spread = boss.Config.HealVfxSpread;
		Vector2 pos = _vfxOrigin + new Vector2((float)GD.RandRange(-spread, spread), (float)GD.RandRange(-spread, spread));
		Vfx.ExplanationHeal.Throw(new VfxExplanationHealData { GlobalPosition = pos });

		_vfxThrown++;
		if (_vfxThrown >= boss.Config.HealVfxBurstCount)
			_finished = true;
	}
}
