
using Godot.Collections;

public readonly struct VfxExplanationBuffData : IVfxData
{
    public required int BuffIndex { get; init; }
    public required Vector2 GlobalPosition { get; init; }
}

public partial class VfxExplanationBuff : VfxExplanation, IVfxNode<VfxExplanationBuffData>
{
    [Export] private Array<Texture2D> _buffTextures;
    [Export] private Sprite2D _spriteNode;
 
    const float Duration = 0.5f;
    
    public void Initialize(VfxExplanationBuffData data)
    {
        float duration = GetRandomizedDuration(Duration);
        GlobalPosition = GetRandomizedPosition(data.GlobalPosition);
        _spriteNode.SetTexture(_buffTextures[data.BuffIndex]);
        SetupPositionTween(duration);
        SetupAlphaTween(duration);
        SetupScaleTween(duration, 1);
    }
    
}