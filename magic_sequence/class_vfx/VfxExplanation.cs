
public abstract partial class VfxExplanation : Node2D
{
    readonly Vector2 _centerGlobalPosition = new Vector2(960, 500);
    readonly float _fadeInRatio = 0.3f;

    readonly float _durationRandomRange = 0.2f;
    readonly float _positionRandomRange = 8.0f;
    readonly float _moveSpeedRandomRange = 10.0f;
    readonly float _moveSpeed = 40.0f;
    readonly float _scaleRandomRange = 0.3f;

    [Export] private Timer _timer;

    private Tween _positionTween;
    private Tween _alphaTween;
    private Tween _scaleTween;

    private Callable _returnAction;

    public void InjectReturnAction(Callable returnAction)
    {
        _returnAction = returnAction;
        _timer.Connect(
            Timer.SignalName.Timeout,
            _returnAction
        );
    }

    public void Play()
        => _timer.Start();

    public void Stop()
        => _returnAction.Call();

    protected void SafelyKillTween(Tween tween)
    {
        if (IsInstanceValid(tween))
        {
            tween.Kill();
        }
    }

    protected float GetRandomizedDuration(float baseDuration)
        => baseDuration + (float)GD.RandRange(-_durationRandomRange/2, _durationRandomRange/2);

    protected Vector2 GetRandomizedPosition(Vector2 basePosition)
        => basePosition + new Vector2(
            (float)GD.RandRange(-_positionRandomRange/2, _positionRandomRange/2),
            (float)GD.RandRange(-_positionRandomRange/2, _positionRandomRange/2)
        );

    protected float GetRandomizedScale(float baseScale)
        => Mathf.Max(0f, baseScale + (float)GD.RandRange(-_scaleRandomRange/2, _scaleRandomRange/2));

    protected void SetupPositionTween(float duration)
    {
        SafelyKillTween(_positionTween);

        Vector2 direction = (_centerGlobalPosition - GlobalPosition).Normalized();
        float speed = _moveSpeed + (float)GD.RandRange(-_moveSpeedRandomRange/2, _moveSpeedRandomRange/2);
        float distance = speed * duration;
        Vector2 endPos = GlobalPosition + direction * distance;

        _positionTween = CreateTween()
            .SetIgnoreTimeScale()
            .SetParallel();

        _positionTween.TweenProperty(this, "global_position:x", endPos.X, duration)
            .From(GlobalPosition.X)
            .SetEase(Tween.EaseType.OutIn)
            .SetTrans(Tween.TransitionType.Sine);

        _positionTween.TweenProperty(this, "global_position:y", endPos.Y, duration)
            .From(GlobalPosition.Y)
            .SetEase(Tween.EaseType.InOut)
            .SetTrans(Tween.TransitionType.Sine);
    }

    protected void SetupAlphaTween(float duration)
    {
        SafelyKillTween(_alphaTween);
        _alphaTween = CreateTween()
            .SetIgnoreTimeScale()
            .SetTrans(Tween.TransitionType.Sine);

        _alphaTween.TweenProperty(
            this, "modulate", new Color(this.Modulate, 1f), _fadeInRatio * duration * 0.2
        ).From(new Color(this.Modulate, 0f));
    }

    protected void SetupScaleTween(float duration, float multipleScale)
    {
        SafelyKillTween(_scaleTween);
        float fadeInDuration = _fadeInRatio * duration;
        float fadeOutDuration = duration - fadeInDuration;

        _scaleTween = CreateTween()
            .SetIgnoreTimeScale()
            .SetParallel();

        _scaleTween.TweenProperty(this, "scale:x", multipleScale, fadeInDuration)
            .From(3f * multipleScale)
            .SetTrans(Tween.TransitionType.Quart);

        _scaleTween.TweenProperty(this, "scale:y", multipleScale, fadeInDuration)
            .From(3f * multipleScale)
            .SetTrans(Tween.TransitionType.Elastic);

        _scaleTween.TweenProperty(this, "scale:x", 0f, fadeOutDuration)
            .SetDelay(fadeInDuration)
            .SetTrans(Tween.TransitionType.Quart);

        _scaleTween.TweenProperty(this, "scale:y", 0f, fadeOutDuration)
            .SetDelay(fadeInDuration)
            .SetTrans(Tween.TransitionType.Quart);
    }
}
