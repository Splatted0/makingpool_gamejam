using System.Collections.Generic;
using Godot;

public partial class Player : CharacterBody2D
{
    [Export] public float Speed { get; set; } = 300.0f;
    [Export] private Godot.Collections.Dictionary<Wand, SpriteFrames> _handsByWand;
    [Export] private AnimatedSprite2D _basePlayerSprite;
    [Export] private AnimatedSprite2D _handSprite;

    private bool _hasRightLimit;
    private float _rightLimitX;

    private bool _tintFlashing;
    private float _tintFlashElapsed;
    private float _tintFlashDuration;
    private Color _tintFlashColor;

    // 보스가 웨이브 진입 시 호출. 이 X 지점보다 오른쪽으로는 못 가게 막는다(위아래는 무제한).
    public void SetRightLimit(float x)
    {
        _rightLimitX = x;
        _hasRightLimit = true;
    }

    // 보스 처치 시 호출. 이동 제한을 해제한다.
    public void ClearRightLimit()
    {
        _hasRightLimit = false;
    }

    // 플레이어 스프라이트를 잠깐 특정 색으로 확 틴트했다가 원래 흰색으로 돌아오게 한다(피격/디버프 강조용).
    public void FlashTint(Color color, float duration)
    {
        _tintFlashColor = color;
        _tintFlashDuration = Mathf.Max(duration, 0.01f);
        _tintFlashElapsed = 0f;
        _tintFlashing = true;
    }

    private void UpdateTintFlash(double delta)
    {
        if (!_tintFlashing)
            return;

        _tintFlashElapsed += (float)delta;
        float t = Mathf.Clamp(_tintFlashElapsed / _tintFlashDuration, 0f, 1f);
        Color current = _tintFlashColor.Lerp(Colors.White, t);

        if (_basePlayerSprite != null)
            _basePlayerSprite.Modulate = current;
        if (_handSprite != null)
            _handSprite.Modulate = current;

        if (t >= 1f)
            _tintFlashing = false;
    }

    public override void _Ready()
    {
        AddToGroup("player");
        Blackboard.WandManager.LanchedWand += OnLanchedWand;
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 direction = Input.GetVector("move_left", "move_right", "move_up", "move_down");

        if (direction != Vector2.Zero)
            Velocity = direction * Speed;
        else
            Velocity = Velocity.MoveToward(Vector2.Zero, Speed);

        MoveAndSlide();

        if (_hasRightLimit && GlobalPosition.X > _rightLimitX)
            GlobalPosition = new Vector2(_rightLimitX, GlobalPosition.Y);

        UpdateTintFlash(delta);
    }

    private async void OnLanchedWand(Wand wand)
    {
        StringName wandName = wand.WandName;
        foreach (KeyValuePair<Wand, SpriteFrames> wandSprite in _handsByWand)
        {
            if (wandSprite.Key.WandName == wandName)
            {
                
                _handSprite.SpriteFrames = wandSprite.Value;
            }
        }
        await PlayAnimation("attack");
        PlayAnimation("idle");
    }
    
    private async Task PlayAnimation(string animationName)
    {
        _basePlayerSprite.Play(animationName);
        _handSprite.Play(animationName);
        await ToSignal(_basePlayerSprite, "animation_finished");
    }
}
