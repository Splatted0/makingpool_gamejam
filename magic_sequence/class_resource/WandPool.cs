[GlobalClass]
public partial class WandPool : DropPool<Wand>
{
    [Export] private Godot.Collections.Array<Wand> _pool = new();
    [Export] private Wand _fallbackDrop;

    protected override Godot.Collections.Array<Wand> CurrentPool => _pool;
    protected override Wand FallbackDrop => _fallbackDrop;
}
