
[GlobalClass]
public partial class Magic : Resource, IDropObject
{ 
    [Export] public string Name { get; private set; }
    [Export] public Texture2D Icon { get; private set; }
    [Export(PropertyHint.MultilineText)] public string Description { get; private set; }
    [Export(PropertyHint.MultilineText)] public string EnhancedDescription { get; private set; }
    [Export] public Tier Tier { get; private set; }
    [Export] public MagicEffect MagicEffect { get; private set; }
}   
