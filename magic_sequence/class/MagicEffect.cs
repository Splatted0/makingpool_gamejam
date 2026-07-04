[GlobalClass]
public abstract partial class MagicEffect : Resource
{
    public abstract void MagicEnhance();
    public bool IsEnhanced { get; protected set; }
}