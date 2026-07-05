using System.Collections.Generic;

public partial class SfxLayer : Node
{
    [Export] public SfxOneShotPool OneShotPool { get; private set; }

    private IEnumerable<ISfx> _sfxCollection;

    public void Clear()
    {
        foreach(ISfx sfx in _sfxCollection)
        {
            sfx.Clear();
        }
    }

    public override void _Ready()
    {
        Sfx.Instance = this;
        _sfxCollection = GetChildren()
                .OfType<ISfx>()
                .ToList();
    }
}
