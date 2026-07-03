using Godot;
using System.Threading.Tasks;

public partial class RoundManager : Node
{
    [Export] public Spawner Spawner { get; set; }
    [Export] public BattleWorldHud Hud { get; set; }

    [Export] public double IntroSeconds { get; set; } = 2.0;
    [Export] public double BetweenRoundSeconds { get; set; } = 1.0;

    [Export] public int RoundNumber { get; set; } = 1;

    // 0이면 무한 라운드
    [Export] public int MaxRounds { get; set; } = 0;

    private bool _started;

    public override void _Ready()
    {
        _ = RunRounds();
    }

    private void ResolveReferences()
    {
        if (Spawner == null)
            Spawner = GetNodeOrNull<Spawner>("../EnemySpawner/Spawner");

        if (Hud == null)
            Hud = GetNodeOrNull<BattleWorldHud>("../../BattleUI");
    }

    private async Task RunRounds()
    {
        if (_started)
            return;

        _started = true;

        // BattleUI가 RoundLabel 만드는 시간 확보
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        ResolveReferences();

        if (Spawner == null)
        {
            GD.PrintErr("[RoundManager] Spawner not found.");
            return;
        }

        if (Hud == null)
        {
            GD.PrintErr("[RoundManager] Hud not found. Round label will be skipped.");
        }

        while (MaxRounds <= 0 || RoundNumber <= MaxRounds)
        {
            WaveData wave = Spawner.GetDefaultWave();

            if (wave == null)
            {
                GD.PrintErr("[RoundManager] WaveData가 없습니다. Spawner의 _testWave에 WaveData를 넣어야 합니다.");
                return;
            }

            GD.Print($"[RoundManager] Round {RoundNumber} intro.");

            if (Hud != null)
                await Hud.ShowRoundIntro(RoundNumber, IntroSeconds);
            else
                await ToSignal(GetTree().CreateTimer(IntroSeconds), SceneTreeTimer.SignalName.Timeout);

            GD.Print($"[RoundManager] Round {RoundNumber} spawn start.");

            Spawner.SpawnStart(wave);

            // 1. 스폰 큐가 끝날 때까지 대기
            await ToSignal(Spawner, Spawner.SignalName.SpawnFinished);

            GD.Print($"[RoundManager] Round {RoundNumber} spawn finished. Waiting for monsters.");

            // 2. 이미 스폰된 몬스터가 전부 죽거나 Core에 박아서 사라질 때까지 대기
            await WaitUntilAllMonstersGone();

            GD.Print($"[RoundManager] Round {RoundNumber} cleared.");

            RoundNumber++;

            await ToSignal(GetTree().CreateTimer(BetweenRoundSeconds), SceneTreeTimer.SignalName.Timeout);
        }

        GD.Print("[RoundManager] All rounds finished.");
    }

    private async Task WaitUntilAllMonstersGone()
    {
        while (CountAliveMonsters() > 0)
        {
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        }
    }

    private int CountAliveMonsters()
    {
        if (Spawner == null || Spawner.Container == null)
            return 0;

        int count = 0;

        foreach (Node child in Spawner.Container.GetChildren())
        {
            if (child is Monster && IsInstanceValid(child))
                count++;
        }

        return count;
    }
}