using System;
using System.Collections.Generic;

public class DebuffController
{
    private const int MaxStacks = 3;

    private readonly Monster _owner;
    private readonly Dictionary<Type, IDebuff> _active = new();

    public DebuffController(Monster owner)
    {
        _owner = owner;
    }

    public static IDebuff Create(Elemental element)
    {
        return element switch
        {
            Elemental.Fire => new FireEffect(),
            Elemental.Ice => new IceEffect(),
            Elemental.Earth => new EarthEffect(),
            _ => null
        };
    }

    public void Apply(IDebuff debuff)
    {
        if (debuff == null)
            return;

        Type key = debuff.GetType();

        if (_active.TryGetValue(key, out IDebuff existing))
        {
            int stacks = Math.Min(existing.Stacks + 1, MaxStacks);
            float duration = Math.Max(existing.Duration, debuff.Duration);

            existing.OnExpire(_owner);
            debuff.Stacks = stacks;
            debuff.Duration = duration;
            _active[key] = debuff;
            debuff.OnApply(_owner);
            return;
        }

        _active[key] = debuff;
        debuff.OnApply(_owner);
    }

    public void Tick(float delta)
    {
        List<Type> expired = null;

        foreach (KeyValuePair<Type, IDebuff> pair in _active)
        {
            pair.Value.Duration -= delta;
            pair.Value.OnTick(_owner, delta);

            if (pair.Value.Duration <= 0f)
            {
                expired ??= new List<Type>();
                expired.Add(pair.Key);
            }
        }

        if (expired == null)
            return;

        foreach (Type key in expired)
        {
            _active[key].OnExpire(_owner);
            _active.Remove(key);
        }
    }
}
