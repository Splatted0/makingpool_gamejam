
public readonly struct VfxExplanationDamageData : IVfxData
{
    public required Vector2 GlobalPosition { get; init; }
    public required int Damage { get; init; }
    public required Color Color { get; init; }
}

public partial class VfxExplanationDamage : VfxExplanation, IVfxNode<VfxExplanationDamageData>
{
    const float Duration = 0.8f;
    const float DamageScaleMultiplier = 0.005f;

    [Export] private RichTextLabel _text;

    public void Initialize(VfxExplanationDamageData data)
    {
        float duration = GetRandomizedDuration(Duration);
        GlobalPosition = GetRandomizedPosition(data.GlobalPosition);
        float multipleScale = GetRandomizedScale(1 + (data.Damage -1) * DamageScaleMultiplier);
        _text.Text = "-" + data.Damage.ToString();
        _text.SelfModulate = data.Color;
        SetupPositionTween(duration);
        SetupAlphaTween(duration);
        SetupScaleTween(duration, multipleScale);
    }
}
