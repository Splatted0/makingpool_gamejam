
using System.Collections.Generic;

public abstract partial class MagicPerk : MagicEffect
{
    public virtual void ApplyToNode(MagicNode node) { }

    public virtual void OnNodeSpawned(MagicNode node) { }

    public virtual void OnNodeMove(MagicNode node, List<Monster> targets, float delta) { }

    public virtual void OnNodeArrival(MagicNode node, List<Monster> targets, int progressedFrame) { }

    public virtual float GetFireCooldownMultiplier() => 1f;

    public virtual float GetSlotDelayMultiplier() => 1f;
}
