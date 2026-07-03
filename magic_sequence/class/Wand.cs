
using Godot.Collections;

[GlobalClass]
public partial class Wand : Resource
{
    [Export] public string WandName { get; private set; } = "";
    [Export] public string Description { get; private set; } = "";
    [Export] public int Slot { get; private set; } = 5;
    [Export] public MagicPerk WandPerk { get; private set; }
    [Export] public Array<Magic> Magics { get; set; } = new();

    public void Setup()
    {
        Magics.Resize(Slot);
    }

    // index 슬롯이 비어 있으면 magic을 넣는다.
    public bool Add(Magic magic, int index)
    {
        if (!IsValidIndex(index) || Magics[index] != null) return false;
        Magics[index] = magic;
        return true;
    }

    // 비어 있는 첫 번째 슬롯에 magic을 넣는다.
    public bool Add(Magic magic)
    {
        for (int i = 0; i < Slot; i++)
        {
            if (Magics[i] == null)
            {
                Magics[i] = magic;
                return true;
            }
        }
        return false;
    }

    // index 슬롯의 magic을 제거하고 반환한다.
    public Magic Remove(int index)
    {
        if (!IsValidIndex(index)) return null;
        var magic = Magics[index];
        Magics[index] = null;
        return magic;
    }

    // from 슬롯과 to 슬롯의 magic을 교환한다.
    public bool Swap(int from, int to)
    {
        if (!IsValidIndex(from) || !IsValidIndex(to)) return false;
        (Magics[from], Magics[to]) = (Magics[to], Magics[from]);
        return true;
    }

    public Magic Get(int index) => IsValidIndex(index) ? Magics[index] : null;

    public Array<Magic> GetAll() => Magics;

    public bool IsEmpty(int index) => IsValidIndex(index) && Magics[index] == null;

    private bool IsValidIndex(int index) => index >= 0 && index < Slot;
}
