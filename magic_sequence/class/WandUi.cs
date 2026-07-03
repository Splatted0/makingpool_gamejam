
using Godot;
using Godot.Collections;

public partial class WandUi : Control
{
    [ExportCategory("Node")]
    [Export] private RichTextLabel _name;
    [Export] private RichTextLabel _description;
    [Export] private Array<MagicPanel> _magicPanel;

    private Wand _wand;

    public override void _Ready()
    {
        foreach (var panel in _magicPanel)
            panel.DragedMagic += OnMagicPanelDragedMagic;
    }

    public void Setup(Wand wand)
    {
        _wand = wand;
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

    private void OnMagicPanelDragedMagic(Magic magic)
    {

    }
}
