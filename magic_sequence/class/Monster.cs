public partial class Monster : CharacterBody2D, IEntity
{
    [Export] public MonsterData Data { get; set; }
    [Export] private AnimatedSprite2D _animatedSprite;

    public Team Team { get; set; } = Team.Enemy;
    public int Health { get; set; }

    private const float MeleeAttackRange = 50f;
    private const int MeleeCollisionLayer = 5;
    private const int RangedCollisionLayer = 6;
    private const float KnockbackDecay = 1800f;
    private const float HealInterval = 1f;
    private const float HealRange = 400f;

    private bool _hasTarget;
    private Vector2 _targetPosition;
    private Vector2 _direction = Vector2.Left;
    private IEntity _core;
    private Node2D _targetNode;
    private bool _isAttacking;
    private double _attackTimer;
    private Vector2 _knockbackVelocity;
    private double _healTimer;

    public bool IsStunned { get; set; }
    public float MoveSpeedMultiplier { get; set; } = 1f;
    public float DamageTakenMultiplier { get; set; } = 1f;

    private DebuffController _debuffs;
    private MonsterAnimator _animator;
    private bool _isDying;

    protected bool IsMelee => Data.AttackRange < 0f;
    protected bool IsHealer => Data.HealAmount > 0;

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
        if (hitInfo.Element == Elemental.Fire)
            TakeDamage(hitInfo.Damage, ColorPreset.Red);
        if (hitInfo.Element == Elemental.Earth)
            TakeDamage(hitInfo.Damage, ColorPreset.Brown);
        if (hitInfo.Element == Elemental.Wind)
            TakeDamage(hitInfo.Damage, ColorPreset.Green);
        if (hitInfo.Element == Elemental.Ice)
            TakeDamage(hitInfo.Damage, ColorPreset.Blue);
        ApplyDebuffs(hitInfo);
    }

    private void ApplyDebuffs(HitInfo hitInfo)
    {
        if (!hitInfo.SuppressElementEffect)
            ApplyElementEffect(hitInfo.Element, hitInfo);

        if (hitInfo.ApplyFireEffect)
            ApplyFireBurn(hitInfo.FireDurationMultiplier);

        if (hitInfo.ApplyIceEffect)
            ApplyIceSlow();

        if (hitInfo.ApplyEarthEffect)
            ApplyEarthStun(hitInfo.EarthDurationMultiplier);

        if (hitInfo.ApplyVulnerableEffect)
            _debuffs.Apply(new VulnerableEffect(2f, 1.1f));
    }

    public virtual void TakeDamage(int amount, Color color)
    {
        if (_isDying)
            return;

        int damage = Mathf.RoundToInt(amount * DamageTakenMultiplier);
        Health -= damage;

        Vfx.ExplanationDamage.Throw(new VfxExplanationDamageData
        {
            GlobalPosition = GlobalPosition,
            Damage = damage,
            Color = color
        });

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

        int typeLayer = IsMelee ? MeleeCollisionLayer : RangedCollisionLayer;
        SetCollisionLayerValue(typeLayer, true);
        CollisionMask = 0;
        SetCollisionMaskValue(typeLayer, true);

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

        if (IsHealer)
            HealTick(delta);

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

        Vfx.DeathParticle.Throw(new VfxDeathParticleData
        {
            GlobalPosition = GlobalPosition
        });

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

    private void ApplyElementEffect(Elemental element, HitInfo hitInfo)
    {
        switch (element)
        {
            case Elemental.Fire:
                ApplyFireBurn(hitInfo.FireDurationMultiplier);
                break;
            case Elemental.Ice:
                ApplyIceSlow();
                break;
            case Elemental.Earth:
                ApplyEarthStun(hitInfo.EarthDurationMultiplier);
                break;
        }
    }

    private void ApplyFireBurn(float durationMultiplier = 1f)
    {
        if (durationMultiplier <= 0f)
            durationMultiplier = 1f;

        _debuffs.Apply(new FireEffect(2f * durationMultiplier, 40));
    }

    private void ApplyIceSlow()
    {
        _debuffs.Apply(new IceEffect(2f, 0.3f));
    }

    private void ApplyEarthStun(float durationMultiplier = 1f)
    {
        if (durationMultiplier <= 0f)
            durationMultiplier = 1f;

        _debuffs.Apply(new EarthEffect(1f * durationMultiplier));
    }

    private void HealTick(double delta)
    {
        _healTimer += delta;
        if (_healTimer < HealInterval)
            return;

        _healTimer = 0;
        HealNearbyAllies();
    }

    private void HealNearbyAllies()
    {
        Node2D container = Blackboard.EntityContainer;
        if (container == null)
            return;

        foreach (Node node in container.GetChildren())
        {
            if (node is not Monster ally || ally == this || ally._isDying)
                continue;

            float distance = GlobalPosition.DistanceTo(ally.GlobalPosition);
            if (distance <= HealRange)
                ally.Heal(Data.HealAmount);
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0)
            return;

        Health = Mathf.Min(Health + amount, Data.MaxHealth);
    }
}
