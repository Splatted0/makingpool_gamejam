
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

        Setup();
    }

    public void Setup()
    {
        Wand[] wands = Blackboard.Wands;
        for (int i = 0; i < WandUis.Count && i < wands.Length; i++)
        {
            WandUis[i].Setup(wands[i]);
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

    // Wand 데이터만 변경. UI 갱신은 EndDrag에서 일괄 처리.
    private void DropMagic()
    {
        if (_hoveredPanel == null) return;

        if (_sourceWand == null)
        {
            if (_hoveredWand.Wand.IsEmpty(_hoveredIndex))
                _hoveredWand.Wand.Add(_addMagic, _hoveredIndex);
            else
                _hoveredWand.Wand.Magics[_hoveredIndex] = _addMagic;
        }
        else if (_sourceWand == _hoveredWand)
        {
            if (_sourceIndex == _hoveredIndex) return;
            _sourceWand.Wand.Swap(_sourceIndex, _hoveredIndex);
        }
        else
        {
            if (_hoveredWand.Wand.IsEmpty(_hoveredIndex))
            {
                _hoveredWand.Wand.Add(_draggedMagic, _hoveredIndex);
                _sourceWand.Wand.Remove(_sourceIndex);
            }
            else
            {
                Magic targetMagic = _hoveredWand.Wand.Get(_hoveredIndex);
                _hoveredWand.Wand.Magics[_hoveredIndex] = _draggedMagic;
                _sourceWand.Wand.Magics[_sourceIndex] = targetMagic;
            }
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

        RefreshAllWandUis();
    }

    private void RefreshAllWandUis()
    {
        foreach (WandUi wandUi in WandUis)
        {
            wandUi.Setup(wandUi.Wand);
        }
    }
}
