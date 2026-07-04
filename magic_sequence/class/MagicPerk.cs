
public abstract partial class MagicPerk : MagicEffect
{
    public virtual void ApplyToNode(MagicNode node) { }

    public virtual void OnNodeSpawned(MagicNode node) { }

    public virtual float GetSlotDelayMultiplier() => 1f;
}
