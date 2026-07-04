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

    private Dictionary<Type, List<MagicPerk>> _perkMap;
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
        HitInfo hitInfo =  new HitInfo();
        hitInfo.Damage = Stat.Damage;
        hitInfo.Element = MagicSpell.Elemental;
        hitInfo.SourceTeam = Team.Player;
        return hitInfo;
    }

    private void OnMagicStatSet()
    {
        float scaleValue =  Stat.Range * 0.1f;
        _moveArea.Scale = new Vector2(scaleValue, scaleValue);
        _arrivalArea.Scale = new Vector2(scaleValue, scaleValue);
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
        _arrivalArea.Visible = false;
        _arrivalArea.Monitoring = false;
        _arrivalArea.Monitorable = false;

        if (_perkMap.TryGetValue(typeof(MagicPerkSpawn), out var spawnPerks))
        {
            foreach (MagicPerk perk in spawnPerks)
                ((MagicPerkSpawn)perk).SpawnEffect(MagicSpell);
        }
        MagicSpell.SpawnEffect(this);
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
            else OnMove(fdelta);
            return;
        }
        OnArrival(fdelta);
        _progressedFrame++;
        if (_progressedFrame >= Stat.DurationFrame) QueueFree();
    }

    public void TriggerArrival()
    {
        if (_arrived) return;
        _arrived = true;

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
        var targets = new List<Monster>();
        foreach (Node2D body in _moveArea.GetOverlappingBodies())
        {
            if (body is Core)
                continue;

            if (body is Monster monster)
                targets.Add(monster);
            else if (body.GetParent() is MagicNode magicNode)
                AffectedElementals.Add(magicNode.MagicSpell.Elemental);
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
