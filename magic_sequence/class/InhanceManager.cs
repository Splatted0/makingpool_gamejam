
using Godot;
using Godot.Collections;

public partial class InhanceManager : Node
{
    [Export] public MagicPanel AddMagicContainer;
    [Export] public Array<WandUi> WandUis;

    private Magic _addMagic;

    private bool _isDragging;
    private Magic _draggedMagic;
    private MagicPanel _sourcePanel;
    private WandUi _sourceWand;
    private int _sourceIndex = -1;

    private MagicPanel _hoveredPanel;
    private WandUi _hoveredWand;
    private int _hoveredIndex = -1;

    public override void _Ready()
    {
        AddMagicContainer.DraggedMagic += OnAddContainerDragged;

        for (int i = 0; i < WandUis.Count; i++)
        {
            WandUi captured = WandUis[i];
            WandUis[i].Dragged += (magic, panelIndex) => OnWandUiDragged(magic, panelIndex, captured);
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (!_isDragging) return;

        if (@event is InputEventMouseMotion)
        {
            UpdateHover();
        }
        else if (@event is InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: false })
        {
            DropMagic();
            EndDrag();
            GetViewport().SetInputAsHandled();
        }
    }

    private void OnAddContainerDragged(Magic _)
    {
        StartDrag(_addMagic, AddMagicContainer, null, -1);
    }

    private void OnWandUiDragged(Magic magic, int panelIndex, WandUi sourceWand)
    {
        StartDrag(magic, sourceWand.MagicPanel[panelIndex], sourceWand, panelIndex);
    }

    private void StartDrag(Magic magic, MagicPanel sourcePanel, WandUi sourceWand, int sourceIndex)
    {
        _isDragging = true;
        _draggedMagic = magic;
        _sourcePanel = sourcePanel;
        _sourceWand = sourceWand;
        _sourceIndex = sourceIndex;
        _sourcePanel.Drag();
    }

    private void UpdateHover()
    {
        Vector2 mousePos = GetViewport().GetMousePosition();

        MagicPanel newHovered = null;
        WandUi newHoveredWand = null;
        int newHoveredIndex = -1;

        for (int w = 0; w < WandUis.Count; w++)
        {
            for (int p = 0; p < WandUis[w].MagicPanel.Count; p++)
            {
                MagicPanel panel = WandUis[w].MagicPanel[p];
                if (!panel.Visible) continue;
                if (!panel.GetGlobalRect().HasPoint(mousePos)) continue;

                newHovered = panel;
                newHoveredWand = WandUis[w];
                newHoveredIndex = p;
                break;
            }
            if (newHovered != null) break;
        }

        if (newHovered == _hoveredPanel) return;

        _hoveredPanel?.Unselected();
        newHovered?.Selected();
        _hoveredPanel = newHovered;
        _hoveredWand = newHoveredWand;
        _hoveredIndex = newHoveredIndex;
    }

    private void DropMagic()
    {
        if (_hoveredPanel == null) return;

        if (_sourceWand == null)
        {
            _hoveredWand.Wand.Magics[_hoveredIndex] = _addMagic;
            _hoveredWand.MagicPanel[_hoveredIndex].Setup(_hoveredWand.Wand.Get(_hoveredIndex));
        }
        else if (_sourceWand == _hoveredWand)
        {
            if (_sourceIndex == _hoveredIndex) return;
            _sourceWand.Wand.Swap(_sourceIndex, _hoveredIndex);
            _sourceWand.MagicPanel[_sourceIndex].Setup(_sourceWand.Wand.Get(_sourceIndex));
            _sourceWand.MagicPanel[_hoveredIndex].Setup(_sourceWand.Wand.Get(_hoveredIndex));
        }
        else
        {
            Magic targetMagic = _hoveredWand.Wand.Get(_hoveredIndex);
            _hoveredWand.Wand.Magics[_hoveredIndex] = _draggedMagic;
            _sourceWand.Wand.Magics[_sourceIndex] = targetMagic;
            _hoveredWand.MagicPanel[_hoveredIndex].Setup(_hoveredWand.Wand.Get(_hoveredIndex));
            _sourceWand.MagicPanel[_sourceIndex].Setup(_sourceWand.Wand.Get(_sourceIndex));
        }
    }

    private void EndDrag()
    {
        _hoveredPanel?.Unselected();
        _sourcePanel?.Able();

        _isDragging = false;
        _draggedMagic = null;
        _sourcePanel = null;
        _sourceWand = null;
        _sourceIndex = -1;
        _hoveredPanel = null;
        _hoveredWand = null;
        _hoveredIndex = -1;
    }
}
