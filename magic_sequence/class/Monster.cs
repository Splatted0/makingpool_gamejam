public partial class Monster : CharacterBody2D, IEntity
{
    [Export] public MonsterData Data { get; set; }
    [Export] private AnimatedSprite2D _animatedSprite;
    [Export] public float DespawnMargin { get; set; } = 96.0f;

    public Team Team { get; set; } = Team.Enemy;
    public int Health { get; set; }

    private const float MeleeAttackRange = 50f;
    private const float KnockbackDecay = 1800f;

    private bool _hasTarget;
    private Vector2 _targetPosition;
    private Vector2 _direction = Vector2.Left;
    private IEntity _core;
    private Node2D _targetNode;
    private bool _isAttacking;
    private double _attackTimer;
    private Vector2 _knockbackVelocity;

    public bool IsStunned { get; set; }
    public float MoveSpeedMultiplier { get; set; } = 1f;
    public float DamageTakenMultiplier { get; set; } = 1f;

    private DebuffController _debuffs;
    private MonsterAnimator _animator;
    private bool _isDying;

    protected bool IsMelee => Data.AttackRange < 0f;

    public void SetTarget(Vector2 target, IEntity core)
    {
        _targetPosition = target;
        _core = core;
        _targetNode = core as Node2D;
        _hasTarget = true;
    }

    public void Hit(HitInfo hitInfo)
    {
        if (hitInfo.SourceTeam == Team)
            return;

        TakeDamage(hitInfo.Damage);

        IDebuff debuff = DebuffController.Create(hitInfo.Element);
        if (debuff != null)
            _debuffs.Apply(debuff);
    }

    public virtual void TakeDamage(int amount)
    {
        Health -= Mathf.RoundToInt(amount * DamageTakenMultiplier);
        if (Health <= 0)
        {
            Die();
            return;
        }

        _animator.PlayHit();
    }

    public override void _Ready()
    {
        _debuffs = new DebuffController(this);

        if (Data == null)
        {
            GD.PrintErr($"[Monster] {Name}: Data is missing.");
            return;
        }

        Health = Data.MaxHealth;

        if (_animatedSprite != null && Data.Frames != null)
            _animatedSprite.SpriteFrames = Data.Frames;

        _animator = new MonsterAnimator(_animatedSprite);

        if (_hasTarget)
        {
            UpdateTargetDirection();
            _animator.PlayWalk();
        }
        else
        {
            GD.PrintErr($"[Monster] {Name}: SetTarget must be called before AddChild.");
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (QueueFreeIfOutsideViewport())
            return;

        _debuffs.Tick((float)delta);

        if (_isDying)
        {
            Velocity = Vector2.Zero;
            MoveAndSlide();
            return;
        }

        if (_knockbackVelocity != Vector2.Zero)
        {
            Velocity = _knockbackVelocity;
            MoveAndSlide();
            _knockbackVelocity = _knockbackVelocity.MoveToward(Vector2.Zero, KnockbackDecay * (float)delta);
            return;
        }

        if (IsStunned)
        {
            Velocity = Vector2.Zero;
            MoveAndSlide();
            return;
        }

        UpdateTargetDirection();
        float distance = GlobalPosition.DistanceTo(_targetPosition);

        if (IsMelee)
            MeleeUpdate(delta, distance);
        else
            RangedUpdate(delta, distance);
    }

    private void MeleeUpdate(double delta, float distance)
    {
        if (distance <= MeleeAttackRange)
        {
            _isDying = true;
            DisableCollision();
            _core.Hit(new HitInfo
            {
                Damage = Health,
                SourceTeam = Team,
                Element = Elemental.None
            });
            _animator.PlaySelfDestruct(QueueFree);
            return;
        }

        Move(delta);
    }

    private void RangedUpdate(double delta, float distance)
    {
        if (distance <= Data.AttackRange)
        {
            _animator.PlayAttack();
            if (!_isAttacking)
            {
                _isAttacking = true;
                _attackTimer = 0;
                Velocity = Vector2.Zero;
                MoveAndSlide();
            }

            AttackInterval(delta);
        }
        else
        {
            _isAttacking = false;
            Move(delta);
        }
    }

    protected virtual void Move(double delta)
    {
        UpdateTargetDirection();
        _animator.PlayWalk();
        Velocity = _direction * Data.MoveSpeed * MoveSpeedMultiplier;
        MoveAndSlide();
    }

    public void Knockback(float amount)
    {
        _knockbackVelocity = -_direction * amount;
    }

    private void AttackInterval(double delta)
    {
        _attackTimer += delta;
        if (_attackTimer < Data.AttackInterval)
            return;

        _attackTimer = 0;
        Attack();
    }

    protected virtual void Attack()
    {
        _core.Hit(new HitInfo
        {
            Damage = Data.AttackDamage,
            SourceTeam = Team,
            Element = Elemental.None
        });
    }

    protected virtual void Die()
    {
        if (_isDying)
            return;

        _isDying = true;
        DisableCollision();

        if (Data != null)
            Blackboard.AddGold(Data.GoldReward);

        _animator.PlayDeath(QueueFree);
    }

    private void DisableCollision()
    {
        CollisionShape2D shape = GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
        shape?.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
    }

    private void UpdateTargetDirection()
    {
        if (_targetNode != null && IsInstanceValid(_targetNode))
            _targetPosition = _targetNode.GlobalPosition;

        Vector2 direction = _targetPosition - GlobalPosition;
        if (direction.LengthSquared() > 0.001f)
            _direction = direction.Normalized();
    }

    private bool QueueFreeIfOutsideViewport()
    {
        Rect2 visibleRect = GetViewport().GetVisibleRect().Grow(DespawnMargin);
        Vector2 screenPosition = GetGlobalTransformWithCanvas().Origin;

        if (visibleRect.HasPoint(screenPosition))
            return false;

        QueueFree();
        return true;
    }
}
