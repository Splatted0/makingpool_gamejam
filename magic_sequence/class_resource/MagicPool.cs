[GlobalClass]
public partial class MagicPool : DropPool<Magic>
{
    [Export] private Godot.Collections.Array<Magic> _pool = new();
    [Export] private Magic _fallbackDrop;

    protected override Godot.Collections.Array<Magic> CurrentPool => _pool;
    protected override Magic FallbackDrop => _fallbackDrop;
}
