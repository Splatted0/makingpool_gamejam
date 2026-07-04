using System.Collections.Generic;

public partial class VfxLayer : CanvasLayer
{
    [Export] public VfxExplanationDamagePool ExplanationDamagePool { get; private set; }
    [Export] public VfxDeathParticlePool DeathParticlePool { get; private set; }
    [Export] public VfxExplanationHealPool ExplanationHealPool { get; private set; }

    private IEnumerable<IVfx> _vfxCollection;

    public void Clear()
    {
        foreach(IVfx vfx in _vfxCollection)
        {
            vfx.Clear();
        }
    }

    public override void _Ready()
    {
        Vfx.Instance = this;
        _vfxCollection = GetChildren()
                .OfType<IVfx>()
                .ToList();
    }
}
