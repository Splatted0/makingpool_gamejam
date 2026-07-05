
public partial class MainMenu : CanvasLayer
{
    [Signal] public delegate void GameStartPressedEventHandler();

    [Export] private BaseButton StartButton;
    [Export] private Control Character;
    [Export] private Control CharacterBackground;
    [Export] private Control Logo;

    [Export] private Color ButtonFlashColor = new Color(1f, 0.8f, 0f);
    [Export] private float ButtonFlashDuration = 0.5f;
    [Export] private Vector2 ButtonFlashScaleMin = new Vector2(0.95f, 0.95f);
    [Export] private Vector2 ButtonFlashScaleMax = new Vector2(1.05f, 1.05f);

    [Export] private float WanderMinDistance = 10f;
    [Export] private float WanderMaxDistance = 50f;
    [Export] private float WanderMinDuration = 0.5f;
    [Export] private float WanderMaxDuration = 2.0f;
    [Export] private float WanderHorizontalBias = 0.2f;
    [Export] private float WanderMinScale = 0.99f;
    [Export] private float WanderMaxScale = 1.01f;

    private Tween _buttonColorTween;
    private Tween _buttonScaleTween;
    private Tween _charTween;
    private Tween _bgTween;

    public override void _Ready()
    {
        StartButton.Pressed += OnStartPressed;
        StartButtonFlash();
        StartCharacterWander();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent &&  mouseEvent.Pressed)
        {
            OnStartPressed();
        }
    }
    
    private void OnStartPressed()
    {
        _buttonColorTween?.Kill();
        _buttonScaleTween?.Kill();
        _charTween?.Kill();
        _bgTween?.Kill();
        EmitSignalGameStartPressed();
    }

    private void StartButtonFlash()
    {
        _buttonColorTween = CreateTween().SetLoops();
        _buttonColorTween.TweenProperty(StartButton, "modulate", ButtonFlashColor, ButtonFlashDuration);
        _buttonColorTween.TweenProperty(StartButton, "modulate", Colors.White, ButtonFlashDuration);

        _buttonScaleTween = CreateTween().SetLoops();
        _buttonScaleTween.TweenProperty(StartButton, "scale", ButtonFlashScaleMax, ButtonFlashDuration);
        _buttonScaleTween.TweenProperty(StartButton, "scale", ButtonFlashScaleMin, ButtonFlashDuration);
    }

    private void StartCharacterWander()
    {
        float distance = (float)GD.RandRange(WanderMinDistance, WanderMaxDistance);
        float duration = (float)GD.RandRange(WanderMinDuration, WanderMaxDuration);
        float angle = GD.Randf() * Mathf.Tau;
        var direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle) * WanderHorizontalBias).Normalized();
        var offset = direction * distance;

        float scale = (float)GD.RandRange(WanderMinScale, WanderMaxScale);
        var scaleVec = new Vector2(scale, scale);

        _charTween = CreateTween().SetParallel();
        _charTween.TweenProperty(Character, "position", Character.Position + offset * 0.1f, duration);
        _charTween.TweenProperty(Character, "scale", scaleVec, duration);

        _bgTween = CreateTween().SetParallel();
        _bgTween.TweenProperty(CharacterBackground, "position", CharacterBackground.Position + offset, duration);
        _bgTween.TweenProperty(CharacterBackground, "scale", scaleVec, duration);
        _bgTween.Chain().TweenCallback(Callable.From(StartCharacterWander));
    }
}
