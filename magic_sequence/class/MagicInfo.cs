
using System.Text.RegularExpressions;
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
        if (_description != null) _description.Text = ResolveDescription(magic.Description, magic);
        if (_tier != null) _tier.SelfModulate = ColorPreset.TierColors[magic.Tier];
    }

    private static string ResolveDescription(string template, Magic magic)
    {
        if (magic == null) return template;
        return Regex.Replace(template, @"\{(\w+)\}", match =>
        {
            string prop = match.Groups[1].Value;

            Variant fromMagic = magic.Get(prop);
            if (fromMagic.VariantType != Variant.Type.Nil) return Format(prop, fromMagic);

            if (magic.MagicEffect != null)
            {
                Variant fromEffect = magic.MagicEffect.Get(prop);
                if (fromEffect.VariantType != Variant.Type.Nil) return Format(prop, fromEffect);
            }

            return match.Value;
        });
    }

    private static string Format(string prop, Variant value)
    {
        if (prop == "BaseDurationFrame")
            return (value.As<int>() / 60f).ToString("0.##");
        return value.ToString();
    }
}
