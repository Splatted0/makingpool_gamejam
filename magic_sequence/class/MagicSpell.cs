using System.Collections.Generic;

public abstract partial class MagicSpell : MagicEffect
{
    [ExportCategory("MagicPack")]
    [Export] public PackedScene MagicNodePack { get; private set; }
    [ExportCategory("MagicStat")]
    [Export] public Elemental Elemental { get; private set; }
    [Export] public float BaseMaxDistance { get; private set; }
    [Export] public float BaseRange { get; private set; }
    [Export] public float BaseSpeed { get; private set; }
    [Export] public float BaseDamage { get; private set; }

    public abstract void SpawnEffect(MagicNode node);

    public abstract void MoveEffect(MagicNode node, float fdelta);

    public abstract void ArrivalEffect(MagicNode node, float fdelta);

    public void MagicEffect(MagicNode node, Elemental effectedElemental)
    {
        
    }
}