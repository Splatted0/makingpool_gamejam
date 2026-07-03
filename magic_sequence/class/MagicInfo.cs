
using Godot;

public partial class MagicInfo : Control
{
    [Export] private Label _name;
    [Export] private RichTextLabel _description;
    [Export] private Panel _tier;

    public void Setup(Magic magic)
    {
        if (magic == null)
        {
            Visible = false;
            return;
        }

        Visible = true;
        if (_name != null) _name.Text = magic.Name;
        if (_description != null) _description.Text = magic.Description;
        if (_tier != null) _tier.SelfModulate = ColorPreset.TierColors[magic.Tier];
    }
}
