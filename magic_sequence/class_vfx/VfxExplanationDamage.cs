
public readonly struct VfxExplanationDamageData : IVfxData
{
    public required Vector2 NobitGlobalPosition { get; init; }
    public required int Damage { get; init; }
}

public partial class VfxExplanationDamage : VfxExplanation, IVfxNode<VfxExplanationDamageData>
{
    const float Duration = 0.8f;
    const float DamageScaleMultiplier = 0.06f;

    [Export] private RichTextLabel _text;

    public void Initialize(VfxExplanationDamageData data)
    {
        float duration = GetRandomizedDuration(Duration);
        GlobalPosition = GetRandomizedPosition(data.NobitGlobalPosition);
        float multipleScale = GetRandomizedScale(1 + (data.Damage -1) * DamageScaleMultiplier);
        _text.Text = data.Damage.ToString();
        SetupPositionTween(duration);
        SetupAlphaTween(duration);
        SetupScaleTween(duration, multipleScale);
    }
}
