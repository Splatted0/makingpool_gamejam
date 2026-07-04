
using Godot;
using Godot.Collections;

public partial class MagicChangeManager : Control
{
    [Signal] public delegate void MagicChangeEndEventHandler();

    [Export] public Array<MagicPanel> GetMagicPanels;
    [Export] public Array<WandUi> WandUis;

    public Magic[] GetMagics { get; private set; }

    public int GetMagicRemainCount
    {
        get
        {
            int count = 0;
            foreach (Magic m in GetMagics)
                if (m != null)
                    count++;
            return count;
        }
    }

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
        GetMagics = new Magic[GetMagicPanels.Count];

        foreach (MagicPanel panel in GetMagicPanels)
        {
            MagicPanel captured = panel;
            captured.DraggedMagic += (magic) => OnAddContainerDragged(magic, captured);
        }

        for (int i = 0; i < WandUis.Count; i++)
        {
            WandUi captured = WandUis[i];
            WandUis[i].Dragged += (magic, panelIndex) => OnWandUiDragged(magic, panelIndex, captured);
        }

        Blackboard.Main.WandsChanged += SyncWandUisFromBlackboard;
    }

    public void Setup(Magic[] magics)
    {
        for (int i = 0; i < GetMagicPanels.Count; i++)
        {
            if (i < magics.Length)
                SetGetMagic(i, magics[i]);
            else
                SetGetMagic(i, null);
        }

        SyncWandUisFromBlackboard();

        if (magics.Length == 0)
            EmitSignal(SignalName.MagicChangeEnd);
    }

    private void SyncWandUisFromBlackboard()
    {
        Wand[] wands = Blackboard.Wands;
        for (int i = 0; i < WandUis.Count; i++)
        {
            Wand wand = i < wands.Length ? wands[i] : null;
            if (wand == null)
            {
                WandUis[i].Visible = false;
                continue;
            }
            WandUis[i].Visible = true;
            WandUis[i].Setup(wand);
        }
    }

    public void SetGetMagic(int index, Magic magic)
    {
        GetMagics[index] = magic;
        if (magic == null)
        {
            GetMagicPanels[index].Visible = false;
        }
        else
        {
            GetMagicPanels[index].Setup(magic);
            GetMagicPanels[index].Visible = true;
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

    private void OnAddContainerDragged(Magic magic, MagicPanel sourcePanel)
    {
        StartDrag(magic, sourcePanel, null, -1);
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
            DropFromGetPanel();
        else if (_sourceWand == _hoveredWand)
            DropWithinSameWand();
        else
            DropBetweenWands();
    }

    private void DropFromGetPanel()
    {
        if (!_hoveredWand.Wand.IsEmpty(_hoveredIndex)) return;

        _hoveredWand.Wand.Add(_draggedMagic, _hoveredIndex);
        int idx = GetMagicPanels.IndexOf(_sourcePanel);
        if (idx >= 0) SetGetMagic(idx, null);
    }

    private void DropWithinSameWand()
    {
        if (_sourceIndex == _hoveredIndex) return;
        _sourceWand.Wand.Swap(_sourceIndex, _hoveredIndex);
    }

    private void DropBetweenWands()
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
        EmitSignal(SignalName.MagicChangeEnd);
    }

    public void RefreshAllWandUis()
    {
        foreach (WandUi wandUi in WandUis)
        {
            wandUi.Setup(wandUi.Wand);
        }
    }
}
