// 주사위 5: 자기 회복. 최소 1, 최대는 현재 잃은 체력만큼 랜덤량으로 즉시 회복한다.
// 풀피여도 연출(틴트+VFX)은 그대로 재생한다(회복량만 0). 힐 "+" VFX는 짧은 간격으로 여러 개 다라락 띄운다.
public class BossDiceHealPattern : IBossPattern
{
	private bool _finished = true;
	private int _vfxThrown;
	private float _vfxElapsed;
	private Vector2 _vfxOrigin;

	public bool IsFinished => _finished;
	public bool WasCancelled => false;

	public void Start(Boss boss)
	{
		boss.PlayBuffAnim();

		int missing = boss.Data.MaxHealth - boss.Health;
		int amount = missing > 0 ? GD.RandRange(1, missing) : 0;
		if (amount > 0)
			boss.Heal(amount);

		boss.FlashTint(new Color(0.4f, 2.2f, 0.3f, 1f), 0.5f);   // 쨍한 연두 틴트로 힐 확 강조(HDR처럼 G채널 1 넘게)
		Sfx.OneShot.Throw(new SfxOneShotData { Stream = boss.Config.Dice5HealSfx });

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
