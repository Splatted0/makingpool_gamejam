using System.Collections.Generic;

public readonly struct VfxCollisionParticleData : IVfxData
{
    public required Vector2 GlobalPosition { get; init; }
    public required Vector2 Impulse { get; init; }
    public required Color Color { get; init; }
}

public partial class VfxCollisionParticle : GpuParticles2D, IVfxNode<VfxCollisionParticleData>
{
    private Callable _returnAction;
    
    public void InjectReturnAction(Callable returnAction)
    {
        _returnAction = returnAction;
        Connect(SignalName.Finished, _returnAction);
    }
    
    public void Initialize(VfxCollisionParticleData data)
    {
        GlobalPosition = data.GlobalPosition;
        GlobalRotation = data.Impulse.Angle() + MathF.PI / 2;
        SelfModulate = data.Color;
    }
    
    public void Play()
        => Restart();
    
    public void Stop()
        => _returnAction.Call();
}
