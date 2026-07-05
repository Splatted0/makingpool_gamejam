using System.Collections.Generic;

// 주사위 4: 코어를 CoreRootDuration초 동안 속박(목줄 무시, 제자리 고정)해 회피 이동을 무력화한다.
// 화면 네 모서리에서 코어로 이어지는 사슬 4가닥을 즉시 출력해 속박을 표현하고, 해제 시 페이드아웃한다.
public class BossDiceRootPattern : IBossPattern
{
	private const float FadeDuration = 0.3f;

	private readonly List<BossChainBeam> _chains = new();
	private float _elapsed;
	private bool _finished = true;

	public bool IsFinished => _finished;
	public bool WasCancelled => false;

	public void Start(Boss boss)
	{
		_elapsed = 0f;
		_finished = false;
		boss.SetCoreRooted(true);
		boss.PlayDebuffAnim();
		boss.FlashTint(new Color(1.6f, 0.3f, 2.2f, 1f), 0.5f);   // 속박 강조: 쨍한 보라 틴트
		SpawnChains(boss);
		GD.Print("[Dice4] 코어 속박");
	}

	public void Tick(Boss boss, double delta)
	{
		if (_finished)
			return;

		// 코어가 제자리에서 흔들리는 동안, 사슬 끝점(시작점은 화면 모서리 고정)이 그 흔들림을 따라가게 갱신.
		Vector2 corePosition = boss.CorePosition;
		foreach (BossChainBeam chain in _chains)
			chain?.UpdateEnd(corePosition);

		_elapsed += (float)delta;
		if (_elapsed >= boss.Config.CoreRootDuration)
		{
			boss.SetCoreRooted(false);

			foreach (BossChainBeam chain in _chains)
				chain?.Fade(FadeDuration);
			_chains.Clear();

			GD.Print("[Dice4] 속박 해제");
			_finished = true;
		}
	}

	// 보스가 속박 도중 죽으면 코어가 영구 속박으로 남지 않게 즉시 해제하고 사슬을 정리한다.
	public void Cancel(Boss boss)
	{
		if (_finished)
			return;

		boss.SetCoreRooted(false);
		foreach (BossChainBeam chain in _chains)
			chain?.Fade(FadeDuration);
		_chains.Clear();
		_finished = true;
	}

	// 화면 네 모서리 각각에서 코어(현재 위치, 속박 중이라 고정)까지 사슬을 하나씩 그린다.
	private void SpawnChains(Boss boss)
	{
		Rect2 rect = boss.GetViewport().GetVisibleRect();
		Vector2 corePosition = boss.CorePosition;
		Vector2[] corners =
		{
			rect.Position,
			new Vector2(rect.End.X, rect.Position.Y),
			new Vector2(rect.Position.X, rect.End.Y),
			rect.End
		};

		Node parent = Blackboard.Main != null ? (Node)Blackboard.EntityContainer : boss.GetParent();

		foreach (Vector2 corner in corners)
		{
			BossChainBeam chain = new BossChainBeam();
			parent.AddChild(chain);
			chain.Setup(corner, corePosition, boss.Config.ChainTexture, boss.Config.RootChainWidth);
			_chains.Add(chain);
		}
	}
}
