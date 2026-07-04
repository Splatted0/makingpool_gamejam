using System.Collections.Generic;

public abstract partial class MagicSpell : MagicEffect
{
    [ExportCategory("MagicPack")]
    [Export] public PackedScene MagicNodePack { get; private set; }
    [ExportCategory("MagicStat")]
    [Export] public Elemental Elemental { get; private set; }
    [Export] public float BaseMaxDistance { get; private set; }
    [Export] public float BaseMoveRange { get; private set; } = 10;
    [Export] public float BaseArrivalRange { get; private set; } = 10;
    [Export] public float BaseSpeed { get; private set; }
    [Export] public int BaseDamage { get; private set; }
    [ExportCategory("MagicData")]
    [Export] public int BaseDurationFrame { get; private set; }

    public abstract void SpawnEffect(MagicNode node);

    public abstract void MoveEffect(MagicNode node, List<Monster> targets, float fdelta);
    
    public abstract void ArrivalEffect(MagicNode node, List<Monster> targetMonster, int progressedFrame);

    public void MagicEffect(MagicNode node, Elemental effectedElemental)
    {
        
    }
}