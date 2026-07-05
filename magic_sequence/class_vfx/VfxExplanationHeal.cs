

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
        SetupScaleTween(duration, 4f);   // 안착 스케일 배율 ↑ — 힐 "+"를 더 크게(잘 안 보여서)
    }
    
}