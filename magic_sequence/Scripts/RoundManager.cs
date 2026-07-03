using Godot;

public partial class RoundManager : Node
{
    [Signal] public delegate void RoundEndedEventHandler();

    [Export] public Spawner Spawner { get; set; }
    [Export] public BattleWorldHud Hud { get; set; }

    [Export] public double IntroSeconds { get; set; } = 2.0;
    [Export] public double BetweenRoundSeconds { get; set; } = 1.0;

    [Export] public int RoundNumber { get; set; } = 1;
    [Export] public int MaxRounds { get; set; } = 0;

    private bool _roundRunning;

    private void ResolveReferences()
    {
        if (Spawner == null)
            Spawner = GetNodeOrNull<Spawner>("../EnemySpawner/Spawner");

        if (Hud == null)
            Hud = GetNodeOrNull<BattleWorldHud>("../..");
    }

    public async void StartRound()
    {
        if (_roundRunning)
            return;

        _roundRunning = true;

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        ResolveReferences();

        if (Spawner == null)
        {
            _roundRunning = false;
            GD.PrintErr("[RoundManager] Spawner not found.");
            return;
        }

        if (Hud == null)
            GD.PrintErr("[RoundManager] Hud not found. Round label will be skipped.");

        if (MaxRounds > 0 && RoundNumber > MaxRounds)
        {
            _roundRunning = false;
            GD.Print("[RoundManager] All rounds finished.");
            return;
        }

        WaveData wave = Spawner.GetDefaultWave();

        if (wave == null)
        {
            _roundRunning = false;
            GD.PrintErr("[RoundManager] WaveData is missing. Assign WaveData to Spawner.");
            return;
        }

        GD.Print($"[RoundManager] Round {RoundNumber} intro.");

        if (Hud != null)
            await Hud.ShowRoundIntro(RoundNumber, IntroSeconds);
        else
            await ToSignal(GetTree().CreateTimer(IntroSeconds), SceneTreeTimer.SignalName.Timeout);

        GD.Print($"[RoundManager] Round {RoundNumber} spawn start.");

        Spawner.SpawnStart(wave);

        await ToSignal(Spawner, Spawner.SignalName.SpawnFinished);

        GD.Print($"[RoundManager] Round {RoundNumber} spawn finished. Waiting for monsters.");

        await WaitUntilAllMonstersGone();

        GD.Print($"[RoundManager] Round {RoundNumber} cleared.");

        RoundNumber++;

        await ToSignal(GetTree().CreateTimer(BetweenRoundSeconds), SceneTreeTimer.SignalName.Timeout);

        _roundRunning = false;
        EmitSignal(SignalName.RoundEnded);
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
