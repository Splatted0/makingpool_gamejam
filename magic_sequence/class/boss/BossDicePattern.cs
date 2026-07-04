// 주사위 1사이클: 가중치로 결과 얼굴을 미리 확정 → 굴러가는 동안 얼굴을 무작위로 빠르게 바꿈 →
// 시간 다 되면 확정 얼굴로 고정. 실제 방해 패턴 디스패치는 다음 단계에서 ResultFace를 참조해 붙인다.
public class BossDicePattern : IBossPattern
{
	private const int FaceCount = 7;

	private float _rollElapsed;
	private float _flickerElapsed;
	private int _targetFace;
	private bool _finished = true;

	public bool IsFinished => _finished;
	public bool WasCancelled => false;
	public int ResultFace => _targetFace;

	public void Start(Boss boss)
	{
		_targetFace = RollWeightedFace(boss.Config);
		_rollElapsed = 0f;
		_flickerElapsed = 0f;
		_finished = false;
		boss.PlayDiceRollAnim();
		GD.Print("[Dice] 굴림 시작");
	}

	public void Tick(Boss boss, double delta)
	{
		if (_finished)
			return;

		BossData data = boss.Config;
		_rollElapsed += (float)delta;
		_flickerElapsed += (float)delta;

		if (_flickerElapsed >= data.DiceFlickerInterval)
		{
			_flickerElapsed = 0f;
			boss.SetDiceFace(GD.RandRange(1, FaceCount));
		}

		if (_rollElapsed >= data.DiceRollDuration)
		{
			boss.SetDiceFace(_targetFace);
			GD.Print($"[Dice] {_targetFace}");
			_finished = true;
		}
	}

	private static int RollWeightedFace(BossData data)
	{
		float[] weights =
		{
			data.DiceWeight1, data.DiceWeight2, data.DiceWeight3, data.DiceWeight4,
			data.DiceWeight5, data.DiceWeight6, data.DiceWeight7
		};

		float total = 0f;
		foreach (float weight in weights)
			total += weight;

		float roll = GD.Randf() * total;
		float cumulative = 0f;
		for (int i = 0; i < weights.Length; i++)
		{
			cumulative += weights[i];
			if (roll <= cumulative)
				return i + 1;
		}

		return weights.Length;
	}
}
