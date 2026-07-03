
public partial class Magic : Resource
{ 
    [Export] public string Name { get; private set; }
    [Export] public Texture2D Icon { get; private set; }
    [Export] public string Description { get; private set; }
    [Export] public Tier Tier { get; private set; }
}   
