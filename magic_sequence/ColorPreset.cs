using System.Collections.Generic;

public static class ColorPreset
{
    
    public static readonly Color White = new Color("EDEDED");
    public static readonly Color GrayWhite = new Color("AFB9CC");
    public static readonly Color Gray = new Color("404C55");
    public static readonly Color BlackGray = new Color("252525");
    public static readonly Color Black = new Color("141414");
    
    public static readonly Color Red = new Color("FF1975");
    public static readonly Color BlackRed = new Color("4D0823");
    public static readonly Color Green = new Color("A8F148");
    public static readonly Color BlackGreen = new Color("354D17");
    public static readonly Color Blue = new Color("17D0E5");
    public static readonly Color Yellow = new Color("FEC640");
    public static readonly Color BlackYellow = new Color("4D3C13");
    public static readonly Color Purple = new Color("E545DD");
    
    public static IReadOnlyDictionary<Tier, Color> TierColors => _tierColors;
    
    private static readonly Dictionary<Tier, Color> _tierColors = new()
    {
        { Tier.Common, Gray },
        { Tier.Rare, Blue },
        { Tier.Epic, Purple },
        { Tier.Legendary, Yellow },
    };
    
}