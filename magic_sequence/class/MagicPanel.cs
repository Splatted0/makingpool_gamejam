
using Godot;

public partial class MagicPanel : Control
{
    [Export] private TextureRect _icon;

    [Signal] public delegate void DragedMagicEventHandler(Magic magic);

    private Magic _magic;
    private bool _dragging;

    public void Setup(Magic magic)
    {
        _magic = magic;
        _icon.Texture = magic?.Icon;
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseBtn && mouseBtn.ButtonIndex == MouseButton.Left)
        {
            _dragging = mouseBtn.Pressed;
        }
        else if (@event is InputEventMouseMotion && _dragging && _magic != null)
        {
            _dragging = false;
            EmitSignal(SignalName.DragedMagic, _magic);
        }
    }
}
