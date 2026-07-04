using Godot.Collections;
using System.Collections.Generic;

public partial class WandManager : Node
{
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
    private readonly Queue<MagicNode>[] _queuedMagicNodes =
    {
        new Queue<MagicNode>(),
        new Queue<MagicNode>(),
        new Queue<MagicNode>()
    };

    public override void _Ready()
    {
        ResolveReferences();
        SetupWands();

        ValidateInputAction("first_wand");
        ValidateInputAction("second_wand");
        ValidateInputAction("third_wand");
    }

    public override void _Process(double delta)
    {
        if (Blackboard.BattleWorldHud != null && !Blackboard.BattleWorldHud.Visible)
            return;

        for (int i = 0; i < _fireCooldownLeft.Length; i++)
            _fireCooldownLeft[i] -= delta;

        UpdateFiringSequences(delta);

        if (Input.IsActionJustPressed("first_wand"))
            Fire(0);

        if (Input.IsActionJustPressed("second_wand"))
            Fire(1);

        if (Input.IsActionJustPressed("third_wand"))
            Fire(2);
    }

    public void SetupWands()
    {
        Wand[] wands = Blackboard.Wands;
        WandNode[] nodes = GetWandNodes();

        for (int i = 0; i < nodes.Length; i++)
        {
            Wand wand = wands != null && i < wands.Length ? wands[i] : null;
            nodes[i]?.Setup(wand);
        }

        Arsenal?.Refresh();
    }

    private void Fire(int wandIndex)
    {
        ResolveReferences();

        if (Player == null || Projectiles == null || wandIndex < 0 || wandIndex >= _fireCooldownLeft.Length)
            return;

        if (_isFiringSequence[wandIndex] || _fireCooldownLeft[wandIndex] > 0.0)
            return;

        WandNode wandNode = GetWandNodes()[wandIndex];

        if (wandNode == null)
            return;

        Array<MagicNode> magicNodes = wandNode.Active();

        if (magicNodes.Count == 0)
            return;

        _queuedMagicNodes[wandIndex].Clear();
        foreach (MagicNode magicNode in magicNodes)
            _queuedMagicNodes[wandIndex].Enqueue(magicNode);

        _isFiringSequence[wandIndex] = true;
        _sequenceDelayLeft[wandIndex] = 0.0;
        LaunchNextInSequence(wandIndex);
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
            _fireCooldownLeft[wandIndex] = GetBaseCooldown(wandIndex);
            Arsenal?.Refresh();
            return;
        }

        Launch(queue.Dequeue());
        Arsenal?.Refresh();

        if (queue.Count > 0)
        {
            _sequenceDelayLeft[wandIndex] = GetSlotDelay(wandIndex);
            return;
        }

        _isFiringSequence[wandIndex] = false;
        _fireCooldownLeft[wandIndex] = GetBaseCooldown(wandIndex);
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
        return wand?.BaseCooldown ?? FireCooldown;
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

    private static void ValidateInputAction(string action)
    {
        if (!InputMap.HasAction(action))
            GD.PrintErr($"[WandManager] Input action '{action}' does not exist. Check Project Settings > Input Map.");
    }

    private static Vector2 GetFireDirection() => Vector2.Right;
}
