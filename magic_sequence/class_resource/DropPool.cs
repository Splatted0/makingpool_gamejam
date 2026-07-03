using System.Collections.Generic;

public abstract partial class DropPool<[MustBeVariant] T> : Resource where T : Resource, IDropObject
{
    protected abstract Godot.Collections.Array<T> CurrentPool { get; }
    protected abstract T FallbackDrop { get; }

    private RandomNumberGenerator _rng = new();

    public void InitPool(IEnumerable<T> dropTable)
    {
        _rng.Randomize();
        CurrentPool.Clear();
        foreach (T item in dropTable) { CurrentPool.Add(item); }
        Shuffle();
    }

    public T[] DrawFixedSlots(Func<T, bool>[] filters, Func<T, bool>[] fallbackFilters = null)
    {
        Shuffle();
        T[] results = new T[filters.Length];
        int tail = CurrentPool.Count - 1;

        for (int j = 0; j < filters.Length; j++)
        {
            bool found = false;

            for (int i = tail; i >= 0; i--)
            {
                if (filters[j](CurrentPool[i]))
                {
                    results[j] = CurrentPool[i];
                    (CurrentPool[i], CurrentPool[tail]) = (CurrentPool[tail], CurrentPool[i]);
                    tail--;
                    found = true;
                    break;
                }
            }

            if (!found && fallbackFilters != null && j < fallbackFilters.Length)
            {
                for (int i = tail; i >= 0; i--)
                {
                    if (fallbackFilters[j](CurrentPool[i]))
                    {
                        results[j] = CurrentPool[i];
                        (CurrentPool[i], CurrentPool[tail]) = (CurrentPool[tail], CurrentPool[i]);
                        tail--;
                        found = true;
                        break;
                    }
                }
            }

            if (!found)
            {
                results[j] = FallbackDrop?.Duplicate() as T;
            }
        }
        return results;
    }

    private void Shuffle()
    {
        int n = CurrentPool.Count;
        while (n > 1)
        {
            n--;
            int k = (int)(_rng.Randi() % (uint)(n + 1));
            (CurrentPool[k], CurrentPool[n]) = (CurrentPool[n], CurrentPool[k]);
        }
    }
}
