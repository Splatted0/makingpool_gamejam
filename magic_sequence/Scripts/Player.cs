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

    // 보스가 웨이브 진입 시 호출. 이 X 지점보다 오른쪽으로는 못 가게 막는다(위아래는 무제한).
    public void SetRightLimit(float x)
    {
        _rightLimitX = x;
        _hasRightLimit = true;
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
