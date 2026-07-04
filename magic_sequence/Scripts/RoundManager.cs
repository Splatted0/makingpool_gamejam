using Godot;

public partial class RoundManager : Node
{
    [Signal] public delegate void RoundEndedEventHandler();

    private const float WaveSpawnInterval = 0.2f;
    private const string SlimePath = "res://resource_ingame/resource_monster/slime.tres";
    private const string SkeletonArcherPath = "res://resource_ingame/resource_monster/skeleton_archer.tres";
    private const string SkeletonShieldPath = "res://resource_ingame/resource_monster/skeleton_shield.tres";
    private const string WolfPath = "res://resource_ingame/resource_monster/wolf.tres";
    private const string FairyHealerPath = "res://resource_ingame/resource_monster/fairy_healer.tres";
    private const string BossDataPath = "res://resource_ingame/resource_monster/boss_data.tres";

    [Export] public Spawner Spawner { get; set; }
    [Export] public BattleWorldHud Hud { get; set; }
    [Export] public Node Projectiles { get; set; }
    [Export] public Label RoundStartLabel { get; set; }

    [Export] public double IntroSeconds { get; set; } = 1.0;
    [Export] public double BetweenRoundSeconds { get; set; } = 1.0;

    [Export] public int RoundNumber { get; set; } = 1;
    [Export] public int MaxRounds { get; set; } = 10;

    private MonsterData _slime;
    private MonsterData _skeletonArcher;
    private MonsterData _skeletonShield;
    private MonsterData _wolf;
    private MonsterData _fairyHealer;
    private MonsterData _bossData;

    private bool _roundRunning;
    private bool _cancelRequested;

    private void ResolveReferences()
    {
        if (Spawner == null)
            Spawner = GetNodeOrNull<Spawner>("../EnemySpawner/Spawner");

        if (Hud == null)
            Hud = GetNodeOrNull<BattleWorldHud>("../..");

        if (Projectiles == null)
            Projectiles = GetNodeOrNull<Node>("../Projectiles");

        if (RoundStartLabel == null)
            RoundStartLabel = GetNodeOrNull<Label>("../../RoundStartLabel");
    }

    public async void StartRound()
    {
        if (_roundRunning)
            return;

        _roundRunning = true;
        _cancelRequested = false;

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        ResolveReferences();

        if (Spawner == null)
        {
            _roundRunning = false;
            GD.PrintErr("[RoundManager] Spawner not found.");
            return;
        }

        if (RoundStartLabel == null)
            GD.PrintErr("[RoundManager] RoundStartLabel not found. Round intro label will be skipped.");

        if (MaxRounds > 0 && RoundNumber > MaxRounds)
        {
            _roundRunning = false;
            GD.Print("[RoundManager] All rounds finished.");
            return;
        }

        WaveData wave = BuildWaveData(RoundNumber);

        if (wave == null)
        {
            _roundRunning = false;
            GD.PrintErr($"[RoundManager] WaveData is missing for round {RoundNumber}.");
            return;
        }

        Blackboard.SetWave(RoundNumber);
        GD.Print($"[RoundManager] Round {RoundNumber} intro.");

        await ShowRoundStartLabel(RoundNumber);

        if (_cancelRequested)
            return;

        GD.Print($"[RoundManager] Round {RoundNumber} spawn start.");

        Spawner.SpawnStart(wave);

        await ToSignal(Spawner, Spawner.SignalName.SpawnFinished);

        if (_cancelRequested)
            return;

        GD.Print($"[RoundManager] Round {RoundNumber} spawn finished. Waiting for monsters.");

        await WaitUntilAllMonstersGone();

        if (_cancelRequested)
            return;

        GD.Print($"[RoundManager] Round {RoundNumber} cleared.");

        RoundNumber++;

        await ToSignal(GetTree().CreateTimer(BetweenRoundSeconds), SceneTreeTimer.SignalName.Timeout);

        if (_cancelRequested)
            return;

        ClearProjectiles();

        _roundRunning = false;
        EmitSignal(SignalName.RoundEnded);
    }

    public void CancelRound()
    {
        _cancelRequested = true;
        _roundRunning = false;
        ResolveReferences();
        if (RoundStartLabel != null)
            RoundStartLabel.Visible = false;
        Spawner?.StopSpawning();
        ClearMonsters();
        ClearProjectiles();
    }

    private async Task ShowRoundStartLabel(int roundNumber)
    {
        if (RoundStartLabel == null)
        {
            await ToSignal(GetTree().CreateTimer(IntroSeconds), SceneTreeTimer.SignalName.Timeout);
            return;
        }

        RoundStartLabel.Text = $"Round {roundNumber}";
        RoundStartLabel.Visible = true;
        RoundStartLabel.MoveToFront();

        await ToSignal(GetTree().CreateTimer(IntroSeconds), SceneTreeTimer.SignalName.Timeout);

        if (IsInstanceValid(RoundStartLabel))
            RoundStartLabel.Visible = false;
    }

    public void ClearProjectiles()
    {
        ResolveReferences();

        if (Projectiles == null)
            return;

        foreach (Node child in Projectiles.GetChildren())
            child.QueueFree();
    }

    private void ClearMonsters()
    {
        if (Spawner == null || Spawner.Container == null)
            return;

        foreach (Node child in Spawner.Container.GetChildren())
        {
            if (child is Monster)
                child.QueueFree();
        }
    }

    private async Task WaitUntilAllMonstersGone()
    {
        while (!_cancelRequested && CountAliveMonsters() > 0)
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
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

    private WaveData BuildWaveData(int roundNumber)
    {
        LoadMonsterData();

        return roundNumber switch
        {
            1 => CreateWave(slime: 15),
            2 => CreateWave(slime: 15, wolf: 10),
            3 => CreateWave(slime: 20, skeletonArcher: 10),
            4 => CreateWave(skeletonArcher: 15, skeletonShield: 15),
            5 => CreateWave(skeletonArcher: 15, skeletonShield: 15, fairyHealer: 3),
            6 => CreateWave(skeletonArcher: 15, wolf: 30),
            7 => CreateWave(slime: 20, skeletonArcher: 15, skeletonShield: 20, fairyHealer: 5),
            8 => CreateWave(slime: 40, wolf: 30),
            9 => CreateWave(skeletonArcher: 25, skeletonShield: 25, fairyHealer: 10),
            10 => CreateBossWave(),
            _ => null
        };
    }

    private void LoadMonsterData()
    {
        _slime ??= GD.Load<MonsterData>(SlimePath);
        _skeletonArcher ??= GD.Load<MonsterData>(SkeletonArcherPath);
        _skeletonShield ??= GD.Load<MonsterData>(SkeletonShieldPath);
        _wolf ??= GD.Load<MonsterData>(WolfPath);
        _fairyHealer ??= GD.Load<MonsterData>(FairyHealerPath);
        _bossData ??= GD.Load<MonsterData>(BossDataPath);
    }

    // 10라운드는 일반 몬스터 없이 보스 단독(1대1) 웨이브.
    private WaveData CreateBossWave()
    {
        return new WaveData
        {
            Interval = WaveSpawnInterval,
            Entries = new Godot.Collections.Array<SpawnEntry>(),
            Boss = _bossData
        };
    }

    private WaveData CreateWave(
        int slime = 0,
        int skeletonArcher = 0,
        int skeletonShield = 0,
        int wolf = 0,
        int fairyHealer = 0)
    {
        var wave = new WaveData
        {
            Interval = WaveSpawnInterval,
            Entries = new Godot.Collections.Array<SpawnEntry>()
        };

        AddEntry(wave, _slime, slime);
        AddEntry(wave, _skeletonArcher, skeletonArcher);
        AddEntry(wave, _skeletonShield, skeletonShield);
        AddEntry(wave, _wolf, wolf);
        AddEntry(wave, _fairyHealer, fairyHealer);

        return wave;
    }

    private static void AddEntry(WaveData wave, MonsterData data, int count)
    {
        if (count <= 0)
            return;

        if (data == null)
        {
            GD.PrintErr("[RoundManager] MonsterData is missing while building WaveData.");
            return;
        }

        wave.Entries.Add(new SpawnEntry
        {
            Data = data,
            Count = count
        });
    }
}
