
using System.Text.RegularExpressions;
using Godot;

public partial class MagicInfo : Control
{
    [Signal] public delegate void EnhanceButtonPressedEventHandler(Magic magic);
    
    [Export] private Label _name;
    [Export] private RichTextLabel _description;
    [Export] private Panel _tier;
    [Export] private Control _enhanceContainer;
    [Export] private RichTextLabel _enhanceDescription;
    [Export] private Button _enhanceButton;
    
    private Magic _currentMagic;
    
    public void Setup(Magic magic)
    {
        if (magic == null)
        {
            Visible = false;
            return;
        }
        
        _name.Text = magic.Name;
        string description = magic.MagicEffect.IsEnhanced ? magic.EnhancedDescription : magic.Description;
        _description.Text = ResolveDescription(description, magic);
        _enhanceDescription.Text = ResolveDescription(magic.EnhancedDescription, magic);
        _tier.SelfModulate = ColorPreset.TierColors[magic.Tier];
        _enhanceContainer.Visible = !magic.MagicEffect.IsEnhanced;
        Visible = true;
    }

    private static string ResolveDescription(string template, Magic magic)
    {
        if (magic == null || template == null) return template;
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

    private void OnEnhanceButtonPressed()
    {
        if (_currentMagic != null)
        {
            EmitSignalEnhanceButtonPressed(_currentMagic);
        }
    }
}
