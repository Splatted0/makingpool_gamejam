

public readonly struct VfxExplanationHealData : IVfxData
{
    public required Vector2 GlobalPosition { get; init; }
}


public partial class VfxExplanationHeal :  VfxExplanation, IVfxNode<VfxExplanationHealData>
{
    const float Duration = 0.8f;

    [Export] private RichTextLabel _text;

    public void Initialize(VfxExplanationHealData data)
    {
        float duration = GetRandomizedDuration(Duration);
        GlobalPosition = GetRandomizedPosition(data.GlobalPosition);
        _text.Text = "+";
        SetupPositionTween(duration);
        SetupAlphaTween(duration);
        SetupScaleTween(duration, 1);
    }
}