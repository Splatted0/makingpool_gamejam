using System.Collections.Generic;

[GlobalClass]
public partial class MagicNode : Node2D
{
    [Export] private Area2D _moveArea;
    [Export] private Area2D _arrivalArea;
    
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

    public void Setup(MagicSpell magicSpell, List<MagicPerk> magicPerks)
    {
        MagicSpell = magicSpell;
        Stat = MagicStat.From(magicSpell);
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
        float fdelta = (float)delta;
        if (!_arrived)
        {
            Move(fdelta);
            if (_distanceTraveled >= Stat.MaxDistance)
                TriggerArrival();
            else OnMove(fdelta);
        }
        else
            OnArrival(fdelta);
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
            if (body is Monster monster)
                targets.Add(monster);
        }

        if (_perkMap.TryGetValue(typeof(MagicPerkMove), out var movePerks))
        {
            foreach (MagicPerk perk in movePerks)
                ((MagicPerkMove)perk).MoveEffect(fdelta, MagicSpell, targets);
        }

        MagicSpell.MoveEffect(this, targets, fdelta);
    }

    private void OnArrival(float fdelta)
    {
        var targets = new List<Monster>();
        foreach (Node2D body in _arrivalArea.GetOverlappingBodies())
        {
            if (body is Monster monster)
                targets.Add(monster);
        }

        if (_perkMap.TryGetValue(typeof(MagicPerkArrival), out var arrivalPerks))
        {
            foreach (MagicPerk perk in arrivalPerks)
                ((MagicPerkArrival)perk).ArrivalEffect(MagicSpell, targets);
        }
        MagicSpell.ArrivalEffect(this, targets, fdelta);
    }
}