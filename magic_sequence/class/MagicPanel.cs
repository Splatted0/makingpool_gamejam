
using Godot;

public partial class MagicPanel : Control
{
    [Export] private TextureRect _icon;

    [Signal] public delegate void DraggedMagicEventHandler(Magic magic);

    private Magic _magic;
    private bool _dragging;

    public void Setup(Magic magic)
    {
        _magic = magic;
        _icon.Texture = magic?.Icon;
    }

    public void Able() => Modulate = Colors.White;

    public void Disable() => Modulate = new Color(0, 0, 0, 1);

    public void Drag() => Modulate = new Color(1, 1, 1, 0.5f);

    public void Selected() => SelfModulate = new Color(0.5f, 1, 0.5f, 1);
    
    public void Unselected() => SelfModulate = Colors.White;

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseBtn && mouseBtn.ButtonIndex == MouseButton.Left)
        {
            _dragging = mouseBtn.Pressed;
        }
        else if (@event is InputEventMouseMotion && _dragging && _magic != null)
        {
            _dragging = false;
            EmitSignal(SignalName.DraggedMagic, _magic);
        }
    }
}
