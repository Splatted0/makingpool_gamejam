
using Godot;
using Godot.Collections;

public partial class WandUi : Control
{
    [ExportCategory("Node")]
    [Export] private RichTextLabel _name;
    [Export] private RichTextLabel _description;
    [Export] private Array<MagicPanel> _magicPanel;
//임시 export
    [Export] private Wand _wand;
    private bool _isDragging;
    private int _dragSourceIndex = -1;
    private Magic _draggedMagic;

    public override void _Ready()
    {
        for (int i = 0; i < _magicPanel.Count; i++)
        {
            int capturedIndex = i;
            _magicPanel[i].DraggedMagic += magic => OnMagicPanelDraggedMagic(magic, capturedIndex);
        }
        Setup(_wand);
    }

    public override void _Input(InputEvent @event)
    {
        if (!_isDragging) return;
        if (@event is not InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true }) return;

        Vector2 mousePos = GetViewport().GetMousePosition();

        for (int i = 0; i < _magicPanel.Count; i++)
        {
            if (!_magicPanel[i].Visible) continue;
            if (!_magicPanel[i].GetGlobalRect().HasPoint(mousePos)) continue;

            if (i != _dragSourceIndex)
            {
                _wand.Swap(_dragSourceIndex, i);
                _magicPanel[_dragSourceIndex].Setup(_wand.Get(_dragSourceIndex));
                _magicPanel[i].Setup(_wand.Get(i));
            }

            EndDrag();
            GetViewport().SetInputAsHandled();
            return;
        }

        EndDrag();
        GetViewport().SetInputAsHandled();
    }

    public void Setup(Wand wand)
    {
        _wand = wand;
        wand.Setup();
        _name.Text = wand.WandName;
        _description.Text = wand.Description;

        for (int i = 0; i < _magicPanel.Count; i++)
        {
            bool active = i < wand.Slot;
            _magicPanel[i].Visible = active;
            if (active)
                _magicPanel[i].Setup(i < wand.Magics.Count ? wand.Magics[i] : null);
        }
    }

    private void OnMagicPanelDraggedMagic(Magic magic, int panelIndex)
    {
        _draggedMagic = magic;
        _dragSourceIndex = panelIndex;
        _isDragging = true;
        _magicPanel[panelIndex].Modulate = new Color(1f, 1f, 1f, 0.5f);
    }

    private void EndDrag()
    {
        if (_dragSourceIndex >= 0)
            _magicPanel[_dragSourceIndex].Modulate = Colors.White;
        _isDragging = false;
        _draggedMagic = null;
        _dragSourceIndex = -1;
    }
}
