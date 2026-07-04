using System.Collections.Generic;

public readonly struct VfxSpawnParticleData : IVfxData
{
    public required Vector2 GlobalPosition { get; init; }
}

public partial class VfxSpawnParticle : GpuParticles2D, IVfxNode<VfxSpawnParticleData>
{
    private Callable _returnAction;

    public void InjectReturnAction(Callable returnAction)
    {
        _returnAction = returnAction;
        Connect(SignalName.Finished, _returnAction);
    }

    public void Initialize(VfxSpawnParticleData data)
    {
        GlobalPosition = data.GlobalPosition;
    }

    public void Play()
        => Restart();

    public void Stop()
        => _returnAction.Call();
}