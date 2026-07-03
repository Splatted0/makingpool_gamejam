
public partial class ButtonHoverPanel : BaseButton
{
    [Export] private Panel _hoverPanel;

    [Export] private float _hoverTweenDuration = 0.15f;
    [Export] private float _hoverRotationPeak = -4f;
    [Export] private float _pressTweenDuration = 0.05f;

    [Export] private float _selfHoverRotationPeak = 5f;
    [Export] private Vector2 _selfHoverScalePeak = new Vector2(1.05f, 1.1f);
    [Export] private float _selfHoverTweenDuration = 0.15f;

    [Export] private Vector2 _pressScale = new Vector2(0.93f, 0.97f);
    [Export] private float _pressRotation = 3f;

    [Export] private float _peakRandomMultiplier = 1.0f;
    [Export] private float _hoverPanelOffsetY = 12f;
    [Export] private float _mouseExitDurationMultiplier = 0.5f;

    private Tween _hoverTween;
    private Tween _pressTween;
    private Tween _selfHoverTween;

    public override void _Ready()
    {
        OffsetTransformEnabled = true;
        _hoverPanel.OffsetTransformEnabled = true;

        _hoverPanel.OffsetTransformPivotRatio = new Vector2(0.5f, 0.5f);
        _hoverPanel.Visible = false;

        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseExited;
        ButtonDown += OnButtonDown;
        ButtonUp += OnButtonUp;
    }

    private float RandomSign() => GD.Randf() < 0.5f ? -1f : 1f;

    private float ApplyRandom(float peak) =>
        peak * GD.Randf() * _peakRandomMultiplier * RandomSign();

    private Vector2 ApplyRandomScale(Vector2 peak)
    {
        var delta = peak - Vector2.One;
        return Vector2.One + delta * GD.Randf() * _peakRandomMultiplier;
    }

    private float ApplyRandomRad(float peak) =>
        Mathf.DegToRad(ApplyRandom(peak));

    private void OnMouseEntered()
    {
        _hoverTween?.Kill();
        _hoverPanel.GlobalPosition = GlobalPosition + new Vector2(0, _hoverPanelOffsetY);
        _hoverPanel.Size = Size;
        _hoverPanel.OffsetTransformScale = new Vector2(0f, 1f);
        _hoverPanel.OffsetTransformRotation = ApplyRandomRad(_hoverRotationPeak);
        _hoverPanel.Visible = true;

        _hoverTween = CreateTween().SetParallel().SetEase(Tween.EaseType.Out);
        _hoverTween.TweenProperty(_hoverPanel, "offset_transform_scale", Vector2.One, _hoverTweenDuration)
            .SetTrans(Tween.TransitionType.Back);
        _hoverTween.TweenProperty(_hoverPanel, "offset_transform_rotation", 0f, _hoverTweenDuration)
            .SetTrans(Tween.TransitionType.Cubic);

        _selfHoverTween?.Kill();
        _selfHoverTween = CreateTween().SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
        _selfHoverTween.TweenProperty(this, "offset_transform_rotation", ApplyRandomRad(_selfHoverRotationPeak), _selfHoverTweenDuration);
        _selfHoverTween.Parallel().TweenProperty(this, "offset_transform_scale", ApplyRandomScale(_selfHoverScalePeak), _selfHoverTweenDuration);
        _selfHoverTween.TweenProperty(this, "offset_transform_rotation", 0f, _selfHoverTweenDuration);
        _selfHoverTween.Parallel().TweenProperty(this, "offset_transform_scale", Vector2.One, _selfHoverTweenDuration);
    }

    private void OnMouseExited()
    {
        _hoverTween?.Kill();
        _hoverTween = CreateTween().SetParallel().SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.In);
        _hoverTween.TweenProperty(_hoverPanel, "offset_transform_scale", new Vector2(0f, 1f), _hoverTweenDuration);
        _hoverTween.TweenProperty(_hoverPanel, "offset_transform_rotation", ApplyRandomRad(_hoverRotationPeak), _hoverTweenDuration);
        _hoverTween.Chain().TweenCallback(Callable.From(() => _hoverPanel.Visible = false));

        var exitDuration = _selfHoverTweenDuration * _mouseExitDurationMultiplier;
        _selfHoverTween?.Kill();
        _selfHoverTween = CreateTween().SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
        _selfHoverTween.TweenProperty(this, "offset_transform_rotation", 0f, exitDuration);
        _selfHoverTween.Parallel().TweenProperty(this, "offset_transform_scale", Vector2.One, exitDuration);
    }

    private void OnButtonDown()
    {
        _selfHoverTween?.Kill();
        _pressTween?.Kill();
        _pressTween = CreateTween().SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
        _pressTween.TweenProperty(this, "offset_transform_scale", ApplyRandomScale(_pressScale), _pressTweenDuration);
        _pressTween.Parallel().TweenProperty(this, "offset_transform_rotation", ApplyRandomRad(_pressRotation), _pressTweenDuration);
        _pressTween.TweenProperty(this, "offset_transform_rotation", 0f, _pressTweenDuration)
            .SetTrans(Tween.TransitionType.Back);
    }

    private void OnButtonUp()
    {
        _pressTween?.Kill();
        _pressTween = CreateTween().SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
        _pressTween.TweenProperty(this, "offset_transform_scale", Vector2.One, _pressTweenDuration);
        _pressTween.Parallel().TweenProperty(this, "offset_transform_rotation", 0f, _pressTweenDuration);
    }
}
