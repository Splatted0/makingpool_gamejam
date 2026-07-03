using Godot;

public partial class MagicMissile : Area2D
{
    [Export] public float Speed { get; set; } = 600.0f;
    [Export] public int Damage { get; set; } = 10;
    [Export] public float LifeTime { get; set; } = 2.0f;

    public Vector2 Direction { get; set; } = Vector2.Right;

    private double _age = 0.0;

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
        AreaEntered += OnAreaEntered;
    }

    public override void _Process(double delta)
    {
        GlobalPosition += Direction * Speed * (float)delta;

        _age += delta;
        if (_age >= LifeTime)
        {
            QueueFree();
        }
    }

    private void OnBodyEntered(Node2D body)
    {
        // 예: 나중에 Enemy.cs가 생기면 Enemy.TakeDamage() 호출
        if (body.HasMethod("TakeDamage"))
        {
            body.Call("TakeDamage", Damage);
            QueueFree();
        }
    }

    private void OnAreaEntered(Area2D area)
    {
        // 적 hurtbox가 Area2D일 경우를 대비
        Node parent = area.GetParent();

        if (parent != null && parent.HasMethod("TakeDamage"))
        {
            parent.Call("TakeDamage", Damage);
            QueueFree();
        }
    }
}