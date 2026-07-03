
public partial class MagicNode: Node2D
{
    private MagicSpell MagicSpell;

    [Export] public float Speed { get; set; } = 600.0f;
    [Export] public float MaxTravelDistance { get; set; } = 1000.0f;

    public Vector2 Direction { get; private set; } = Vector2.Right;

    private float _travelledDistance = 0.0f;

    public void Setup(MagicSpell magicSpell)
    {
        MagicSpell = magicSpell;
    }

    public void Setup(Vector2 direction)
    {
        Direction = direction.Normalized();

        if (Direction == Vector2.Zero)
            Direction = Vector2.Right;

        Rotation = Direction.Angle();
    }

    public void Setup(MagicSpell magicSpell, Vector2 direction)
    {
        MagicSpell = magicSpell;
        Setup(direction);
    }

    public void OnSpawn()
    {
        MagicSpell.SpawnEffect();
    }

    public override void _Ready()
    {
        OnSpawn();
    }

    public override void _PhysicsProcess(double delta)
    {
        float fdelta  = (float)delta;
        Move(fdelta);
        OnMove(fdelta);
    }
    
    private void Move(float fdelta)
    {
        float moveDistance = Speed * fdelta;

        GlobalPosition += Direction * moveDistance;
        _travelledDistance += moveDistance;

        if (_travelledDistance >= MaxTravelDistance)
        {
            QueueFree();
        }
    }

    private void OnMove(float fdelta)
    {
        MagicSpell.MoveEffect(fdelta);
    }
}