using System.Collections.Generic;

public readonly struct VfxDeathParticleData : IVfxData
{
    public required Vector2 GlobalPosition { get; init; }
    public required Vector2 Direction { get; init; }
    public required Color Color { get; init; }
}

public partial class VfxDeathParticle : GpuParticles2D, IVfxNode<VfxDeathParticleData>
{
    private Callable _returnAction;

    public void InjectReturnAction(Callable returnAction)
    {
        _returnAction = returnAction;
        Connect(SignalName.Finished, _returnAction);
    }

    public void Initialize(VfxDeathParticleData data)
    {
        GlobalPosition = data.GlobalPosition;
        Rotation = data.Direction.Angle();
        Modulate = data.Color;
    }

    public void Play()
        => Restart();

    public void Stop()
        => _returnAction.Call();
}
