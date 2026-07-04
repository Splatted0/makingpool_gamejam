using Godot.Collections;
using System.Collections.Generic;

public partial class WandManager : Node
{
    [Signal] public delegate void LanchedWandEventHandler(Wand wand);
    [Export] public Player Player { get; set; }
    [Export] public Core Core { get; set; }
    [Export] public Node Projectiles { get; set; }
    [Export] public WandNode WandNode1 { get; set; }
    [Export] public WandNode WandNode2 { get; set; }
    [Export] public WandNode WandNode3 { get; set; }
    [Export] public Arsenal Arsenal { get; set; }

    [Export] public float SpawnDistance { get; set; } = 50.0f;
    [Export] public double FireCooldown { get; set; } = 0.5;

    private readonly double[] _fireCooldownLeft = new double[3];
    private readonly bool[] _isFiringSequence = new bool[3];
    private readonly double[] _sequenceDelayLeft = new double[3];
    private readonly double[] _sequenceCooldownMultiplier = { 1.0, 1.0, 1.0 };
    private readonly Queue<MagicNode>[] _queuedMagicNodes =
    {
        new Queue<MagicNode>(),
        new Queue<MagicNode>(),
        new Queue<MagicNode>()
    };
    private double _cooldownMultiplier = 1.0;   // 주사위7(조커) 등이 재발사 대기시간을 잠시 스케일링할 때 사용

    public override void _Ready()
    {
        ResolveReferences();
        SetupWands();
    }

    public override void _Process(double delta)
    {
        if (Blackboard.BattleWorldHud != null && !Blackboard.BattleWorldHud.Visible)
            return;

        UpdateFiringSequences(delta);

        for (int i = 0; i < _fireCooldownLeft.Length; i++)
        {
            if (_isFiringSequence[i])
                continue;

            _fireCooldownLeft[i] -= delta;

            if (_fireCooldownLeft[i] <= 0.0 && !TryFire(i))
                _fireCooldownLeft[i] = GetBaseCooldown(i);
        }
    }

    public void SetupWands()
    {
        Wand[] wands = Blackboard.Wands;
        WandNode[] nodes = GetWandNodes();

        for (int i = 0; i < nodes.Length; i++)
        {
            Wand wand = wands != null && i < wands.Length ? wands[i] : null;
            wand?.Setup();
            nodes[i]?.Setup(wand);
            _fireCooldownLeft[i] = 0.0;
            _isFiringSequence[i] = false;
            _sequenceDelayLeft[i] = 0.0;
            _sequenceCooldownMultiplier[i] = 1.0;
            _queuedMagicNodes[i].Clear();
        }

        Arsenal?.Refresh();
    }

    // 재발사 대기시간(GetBaseCooldown) 스케일링. 0에 가까울수록 연타 시 거의 바로 재발사된다.
    // 진행 중인 발사 시퀀스(_isFiringSequence)는 건드리지 않으므로 콤보 내부 흐름은 그대로다.
    public void SetCooldownMultiplier(double multiplier)
    {
        _cooldownMultiplier = Math.Max(multiplier, 0.0);
    }

    private bool TryFire(int wandIndex)
    {
        ResolveReferences();

        if (Player == null || Projectiles == null || wandIndex < 0 || wandIndex >= _fireCooldownLeft.Length)
            return false;

        if (_isFiringSequence[wandIndex] || _fireCooldownLeft[wandIndex] > 0.0)
            return false;

        WandNode wandNode = GetWandNodes()[wandIndex];

        if (wandNode == null)
            return false;

        Array<MagicNode> magicNodes = wandNode.Active();

        if (magicNodes.Count == 0)
            return false;

        _queuedMagicNodes[wandIndex].Clear();
        _sequenceCooldownMultiplier[wandIndex] = 1.0;
        foreach (MagicNode magicNode in magicNodes)
        {
            _queuedMagicNodes[wandIndex].Enqueue(magicNode);
            _sequenceCooldownMultiplier[wandIndex] *= Math.Max(magicNode.GetFireCooldownMultiplier(), 0f);
        }

        _isFiringSequence[wandIndex] = true;
        _sequenceDelayLeft[wandIndex] = 0.0;
        LaunchNextInSequence(wandIndex);
        
        EmitSignal(SignalName.LanchedWand, wandNode.Wand);
        return true;
    }

    private void UpdateFiringSequences(double delta)
    {
        for (int i = 0; i < _isFiringSequence.Length; i++)
        {
            if (!_isFiringSequence[i])
                continue;

            _sequenceDelayLeft[i] -= delta;
            if (_sequenceDelayLeft[i] <= 0.0)
                LaunchNextInSequence(i);
        }
    }

    private void LaunchNextInSequence(int wandIndex)
    {
        Queue<MagicNode> queue = _queuedMagicNodes[wandIndex];

        if (queue.Count == 0)
        {
            _isFiringSequence[wandIndex] = false;
            _fireCooldownLeft[wandIndex] = GetBaseCooldown(wandIndex) * _sequenceCooldownMultiplier[wandIndex];
            _sequenceCooldownMultiplier[wandIndex] = 1.0;
            Arsenal?.Refresh();
            return;
        }

        MagicNode launchedNode = queue.Dequeue();
        Launch(launchedNode);
        Arsenal?.Refresh();

        if (queue.Count > 0)
        {
            _sequenceDelayLeft[wandIndex] = GetSlotDelay(wandIndex) * Math.Max(launchedNode.GetSlotDelayMultiplier(), 0f);
            return;
        }

        _isFiringSequence[wandIndex] = false;
        _fireCooldownLeft[wandIndex] = GetBaseCooldown(wandIndex) * _sequenceCooldownMultiplier[wandIndex];
        _sequenceCooldownMultiplier[wandIndex] = 1.0;
    }

    private void Launch(MagicNode magicNode)
    {
        Vector2 direction = GetFireDirection();
        Vector2 spawnPosition = Player.GlobalPosition + direction * SpawnDistance;

        Projectiles.AddChild(magicNode);
        magicNode.GlobalPosition = spawnPosition;
        magicNode.Fire(direction);
        magicNode.OnSpawn();
    }

    public WandNode[] GetWandNodes() => new[] { WandNode1, WandNode2, WandNode3 };

    private double GetBaseCooldown(int wandIndex)
    {
        Wand wand = GetWandNodes()[wandIndex]?.Wand;
        double baseCooldown = wand?.BaseCooldown ?? FireCooldown;
        return baseCooldown * _cooldownMultiplier;
    }

    private double GetSlotDelay(int wandIndex)
    {
        Wand wand = GetWandNodes()[wandIndex]?.Wand;
        float multiplier = 1f;
        if (wand?.WandPerks != null)
            foreach (var perk in wand.WandPerks)
                multiplier *= perk.GetSlotDelayMultiplier();
        return FireCooldown * multiplier;
    }

    private void ResolveReferences()
    {
        Player ??= GetNodeOrNull<Player>("../Player");
        Core ??= GetNodeOrNull<Core>("../Core");
        Projectiles ??= GetNodeOrNull<Node>("../Projectiles");
        Arsenal ??= GetNodeOrNull<Arsenal>("../../UIRoot/Arsenal");
    }

    private static Vector2 GetFireDirection() => Vector2.Right;
}
