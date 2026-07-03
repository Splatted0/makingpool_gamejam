﻿
using System.Collections.Generic;

public partial class MagicNode : Area2D
{
    public MagicSpell MagicSpell { get; private set; }
    public MagicStat Stat;
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

    private static Type GetPerkKey(MagicPerk perk) => perk switch
    {
        MagicPerkSpawn   => typeof(MagicPerkSpawn),
        MagicPerkMove    => typeof(MagicPerkMove),
        MagicPerkArrival => typeof(MagicPerkArrival),
        _                => typeof(MagicPerk)
    };

    public void OnSpawn()
    {
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
            OnMove(fdelta);
        }
        else
            OnArrival(fdelta);
    }

    public void TriggerArrival()
    {
        if (_arrived) return;
        _arrived = true;
    }

    private void Move(float fdelta)
    {
        Vector2 step = Direction * Stat.Speed * fdelta;
        GlobalPosition += step;
        _distanceTraveled += step.Length();

        if (_distanceTraveled >= Stat.MaxDistance)
            TriggerArrival();
    }
    
    private void OnMove(float fdelta)
    {
        var targets = new List<Monster>();
        foreach (Node2D body in GetOverlappingBodies())
        {
            if (body is PhysicsBody2D physBody && physBody.GetCollisionLayerValue(4) && body is Monster monster)
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
        foreach (Node2D body in GetOverlappingBodies())
        {
            if (body is PhysicsBody2D physBody && physBody.GetCollisionLayerValue(4) && body is Monster monster)
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