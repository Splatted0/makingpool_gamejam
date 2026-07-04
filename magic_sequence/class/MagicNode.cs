using System.Collections.Generic;

[GlobalClass]
public partial class MagicNode : Node2D
{
    [Export] private Area2D _moveArea;
    [Export] private Area2D _arrivalArea;
    [Export] public float DespawnMargin { get; set; } = 96.0f;

    public MagicSpell MagicSpell { get; private set; }
    private MagicStat _stat;
    public MagicStat Stat
    {
        get => _stat;
        set { _stat = value; OnMagicStatSet(); }
    }

    public Vector2 Direction { get; set; } = Vector2.Right;
    public List<Elemental> AffectedElementals;
    public Monster PrimaryTarget { get; set; }
    public bool HasSplit { get; set; }

    private Dictionary<Type, List<MagicPerk>> _perkMap;
    private readonly HashSet<ulong> _moveHitIds = new();
    private bool _arrived;
    private float _distanceTraveled;
    private int _progressedFrame;

    public void Setup(MagicSpell magicSpell, List<MagicPerk> magicPerks)
    {
        MagicSpell = magicSpell;
        Stat = MagicStat.From(magicSpell);
        AffectedElementals = new List<Elemental>();
        _perkMap = new Dictionary<Type, List<MagicPerk>>();

        foreach (MagicPerk perk in magicPerks)
        {
            Type key = GetPerkKey(perk);

            if (!_perkMap.TryGetValue(key, out var list))
            {
                list = new List<MagicPerk>();
                _perkMap[key] = list;
            }
            list.Add(perk);
        }
    }

    public void Fire(Vector2 direction)
    {
        Direction = direction.Normalized();

        if (Direction == Vector2.Zero)
            Direction = Vector2.Right;

        Rotation = Direction.Angle();
    }

    public HitInfo GetHitInfo()
    {
        return new HitInfo
        {
            Damage = Stat.Damage,
            Element = MagicSpell.Elemental,
            SourceTeam = Team.Player
        };
    }

    private void OnMagicStatSet()
    {
        SetAreaRange(_moveArea, Stat.Range);
        SetAreaRange(_arrivalArea, Stat.Range);
    }

    private static Type GetPerkKey(MagicPerk perk) => perk switch
    {
        MagicPerkSpawn   => typeof(MagicPerkSpawn),
        MagicPerkMove    => typeof(MagicPerkMove),
        MagicPerkArrival => typeof(MagicPerkArrival),
        _                => typeof(MagicPerk)
    };

    public void OnSpawn()
    {
        OnMagicStatSet();

        if (_moveArea == null || _arrivalArea == null)
        {
            QueueFree();
            return;
        }

        _arrivalArea.Visible = false;
        _arrivalArea.Monitoring = false;
        _arrivalArea.Monitorable = false;

        if (_perkMap.TryGetValue(typeof(MagicPerkSpawn), out var spawnPerks))
        {
            foreach (MagicPerk perk in spawnPerks)
                ((MagicPerkSpawn)perk).SpawnEffect(MagicSpell);
        }
        MagicSpell.SpawnEffect(this);

        if (_perkMap.TryGetValue(typeof(MagicPerk), out var genericPerks))
        {
            foreach (MagicPerk perk in genericPerks)
            {
                if (perk is MagicPerkSplitNextCast splitPerk)
                    splitPerk.SpawnEffect(this);
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (QueueFreeIfOutsideViewport())
            return;

        float fdelta = (float)delta;
        if (!_arrived)
        {
            Move(fdelta);
            if (_distanceTraveled >= Stat.MaxDistance)
                TriggerArrival();
            else
                OnMove(fdelta);
            return;
        }

        OnArrival(fdelta);
        _progressedFrame++;
        if (_progressedFrame >= Stat.DurationFrame)
            QueueFree();
    }

    public void TriggerArrival()
    {
        if (_arrived)
            return;

        _arrived = true;
        _progressedFrame = 0;

        _moveArea.Visible = false;
        _moveArea.Monitoring = false;
        _moveArea.Monitorable = false;

        _arrivalArea.Visible = true;
        _arrivalArea.Monitoring = true;
        _arrivalArea.Monitorable = true;
    }

    private void Move(float fdelta)
    {
        Vector2 step = Direction * Stat.Speed * fdelta;
        GlobalPosition += step;
        _distanceTraveled += step.Length();
    }

    private void OnMove(float fdelta)
    {
        if (_moveArea == null)
            return;

        var targets = new List<Monster>();
        foreach (Node2D body in _moveArea.GetOverlappingBodies())
        {
            if (body is Core)
                continue;

            if (body is Monster monster)
                targets.Add(monster);
            else if (body.GetParent() is MagicNode { MagicSpell: not null } magicNode)
                AddAffectedElemental(magicNode.MagicSpell.Elemental);
        }

        foreach (Area2D area in _moveArea.GetOverlappingAreas())
        {
            if (area.GetParent() is MagicNode { MagicSpell: not null } magicNode)
                AddAffectedElemental(magicNode.MagicSpell.Elemental);
        }

        if (_perkMap.TryGetValue(typeof(MagicPerkMove), out var movePerks))
        {
            foreach (MagicPerk perk in movePerks)
                ((MagicPerkMove)perk).MoveEffect(fdelta, MagicSpell, targets);
        }

        MagicSpell.MoveEffect(this, targets, fdelta);
    }

    private void OnArrival(float fdelta, bool isFirstTrigger = false)
    {
        if (_arrivalArea == null)
            return;

        var targets = new List<Monster>();
        foreach (Node2D body in _arrivalArea.GetOverlappingBodies())
        {
            if (body is Core)
                continue;

            if (body is Monster monster)
                targets.Add(monster);
        }

        if (_perkMap.TryGetValue(typeof(MagicPerkArrival), out var arrivalPerks))
        {
            foreach (MagicPerk perk in arrivalPerks)
                ((MagicPerkArrival)perk).ArrivalEffect(MagicSpell, targets, _progressedFrame);
        }
        MagicSpell.ArrivalEffect(this, targets, _progressedFrame);
    }

    public bool TryMarkMoveHit(Monster monster)
    {
        ulong id = monster.GetInstanceId();
        if (_moveHitIds.Contains(id))
            return false;

        _moveHitIds.Add(id);
        return true;
    }

    public MagicNode SpawnSibling(float angleDegrees)
    {
        MagicNode sibling = MagicSpell.MagicNodePack.Instantiate<MagicNode>();
        sibling.Setup(MagicSpell, new List<MagicPerk>());
        GetParent().AddChild(sibling);
        sibling.GlobalPosition = GlobalPosition;
        sibling.Fire(Direction.Rotated(Mathf.DegToRad(angleDegrees)));
        sibling.OnSpawn();
        sibling.AffectedElementals.AddRange(AffectedElementals);
        sibling.HasSplit = true;
        return sibling;
    }

    private void AddAffectedElemental(Elemental elemental)
    {
        if (elemental == Elemental.None || elemental == MagicSpell.Elemental)
            return;

        if (AffectedElementals.Count > 0 && AffectedElementals[^1] == elemental)
            return;

        AffectedElementals.Add(elemental);
    }

    private static void SetAreaRange(Area2D area, float range)
    {
        if (area == null)
            return;

        area.Scale = Vector2.One;

        foreach (Node child in area.GetChildren())
        {
            if (child is CollisionShape2D collision && collision.Shape != null)
            {
                Shape2D shape = collision.Shape.Duplicate() as Shape2D;
                switch (shape)
                {
                    case CircleShape2D circle:
                        circle.Radius = range;
                        break;
                    case CapsuleShape2D capsule:
                        capsule.Radius = range;
                        capsule.Height = range * 2f;
                        break;
                    case RectangleShape2D rectangle:
                        rectangle.Size = new Vector2(range * 2f, range * 2f);
                        break;
                }
                collision.Shape = shape;
            }
            else if (child is GpuParticles2D gpu)
            {
                gpu.Scale = Vector2.One * (range / 10f);
            }
            else if (child is CpuParticles2D cpu)
            {
                cpu.Scale = Vector2.One * (range / 10f);
            }
        }
    }


    private bool QueueFreeIfOutsideViewport()
    {
        Rect2 visibleRect = GetViewport().GetVisibleRect().Grow(DespawnMargin);
        Vector2 screenPosition = GetGlobalTransformWithCanvas().Origin;

        if (visibleRect.HasPoint(screenPosition))
            return false;

        QueueFree();
        return true;
    }
}
