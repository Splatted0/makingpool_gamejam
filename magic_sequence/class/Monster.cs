public partial class Monster : CharacterBody2D, IEntity
{
    [Export] public MonsterData Data { get; set; }
    [Export] private AnimatedSprite2D _animatedSprite;

    public Team Team { get; set; } = Team.Enemy;
    public int Health { get; set; }

    private bool _hasTarget;
    private Node2D _targetNode;
    private Vector2 _targetPosition;

    private bool _hasHitCore;

    public void SetTarget(Node2D target)
    {
        _targetNode = target;

        if (target != null)
        {
            _targetPosition = target.GlobalPosition;
            _hasTarget = true;
        }
    }

    public void SetTarget(Vector2 target)
    {
        _targetNode = null;
        _targetPosition = target;
        _hasTarget = true;
    }

    public void Hit(HitInfo hitInfo)
    {
        if (hitInfo.SourceTeam == Team)
            return;

        TakeDamage(hitInfo.Damage);
    }

    public virtual void TakeDamage(int amount)
    {
        if (amount <= 0)
            return;

        Health -= amount;

        GD.Print($"[Monster] HP: {Health}");

        if (Health <= 0)
            Die();
    }

    public override void _Ready()
    {
        if (Data == null)
        {
            GD.PrintErr($"[Monster] {Name}: Data가 비어있습니다. MonsterData를 지정해야 합니다.");
            return;
        }

        Health = Data.MaxHealth;

        if (_animatedSprite != null && Data.Frames != null)
        {
            _animatedSprite.SpriteFrames = Data.Frames;
            _animatedSprite.Play();
        }

        if (!_hasTarget)
        {
            GD.PrintErr($"[Monster] {Name}: SetTarget이 호출되지 않았습니다.");
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        MoveTowardCore();
        CheckCoreCollision();
    }

    private void MoveTowardCore()
    {
        if (Data == null || !_hasTarget)
        {
            Velocity = Vector2.Zero;
            MoveAndSlide();
            return;
        }

        Vector2 target = GetTargetPosition();
        Vector2 toTarget = target - GlobalPosition;

        if (toTarget.LengthSquared() < 4f)
        {
            Velocity = Vector2.Zero;
        }
        else
        {
            Velocity = toTarget.Normalized() * Data.MoveSpeed;
        }

        MoveAndSlide();
    }

    private Vector2 GetTargetPosition()
    {
        if (_targetNode != null && IsInstanceValid(_targetNode))
        {
            _targetPosition = _targetNode.GlobalPosition;
        }

        return _targetPosition;
    }

    private void CheckCoreCollision()
    {
        if (_hasHitCore)
            return;

        for (int i = 0; i < GetSlideCollisionCount(); i++)
        {
            KinematicCollision2D collision = GetSlideCollision(i);

            if (collision.GetCollider() is Core core)
            {
                AttackCore(core);
                return;
            }
        }
    }

    protected virtual void AttackCore(Core core)
    {
        if (_hasHitCore)
            return;

        if (core == null || !IsInstanceValid(core))
            return;

        _hasHitCore = true;

        int damage = Data?.AttackDamage ?? 1;
        core.TakeDamage(damage);

        GD.Print($"[Monster] Hit core. Damage: {damage}");

        QueueFree();
    }

    protected virtual void Die()
    {
        QueueFree();
    }
}