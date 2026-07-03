
using Godot;
using Godot.Collections;

public partial class WandUi : Control
{
    [Signal] public delegate void DraggedEventHandler(Magic magic, int panelIndex);

    [ExportCategory("Node")]
    [Export] private RichTextLabel _name;
    [Export] private RichTextLabel _description;
    [Export] public Array<MagicPanel> MagicPanel;

    [Export] private Wand _wand;
    public Wand Wand => _wand;

    public override void _Ready()
    {
        for (int i = 0; i < MagicPanel.Count; i++)
        {
            int capturedIndex = i;
            MagicPanel[i].DraggedMagic += magic => OnMagicPanelDraggedMagic(magic, capturedIndex);
        }
        Setup(_wand);
    }

    public void Setup(Wand wand)
    {
        _wand = wand;
        wand.Setup();
        _name.Text = wand.WandName;
        _description.Text = wand.Description;

        for (int i = 0; i < MagicPanel.Count; i++)
        {
            bool active = i < wand.Slot;
            MagicPanel[i].Visible = active;
            if (active)
            {
                MagicPanel[i].Setup(i < wand.Magics.Count ? wand.Magics[i] : null);
                MagicPanel[i].Able();
            }
            else
            {
                MagicPanel[i].Disable();
            }
        }
    }

    private void OnMagicPanelDraggedMagic(Magic magic, int panelIndex)
    {
        EmitSignal(SignalName.Dragged, magic, panelIndex);
    }
}
