// 몬스터 베이스 클래스. 스탯·외형은 MonsterData 리소스에서, 행동(이동/공격/피격)은 여기서.
// 스탯만 다른 종류는 .tres 교체로 끝. 특수 행동을 가진 종류만 이 클래스를 상속해 override 한다.
public partial class Monster : CharacterBody2D, IEntity
{
    private const string HomeBaseGroup = "home_base";   // 본진이 속한 그룹 이름

    [Export] public MonsterData Data { get; set; }   // 종류별 스탯·외형. 인스펙터에서 .tres를 갈아끼운다
    [Export] private Sprite2D _sprite;               // 외형을 적용할 스프라이트. 씬에서 연결

    // ── IEntity 계약 ──
    public Team Team { get; set; } = Team.Enemy;
    public int Health { get; set; }                  // 런타임 현재 체력. _Ready에서 Data.MaxHealth로 초기화

    private Vector2 _targetPosition;                 // 향할 본진 좌표
    private bool _hasTarget;                          // 타깃이 정해졌는지 (본진)
    private Vector2 _direction;                       // 스폰 시 1회 계산한 직선 방향(본진 고정이라 갱신 안 함)

    // 근거리 여부 — Data.AttackRange가 음수면 접촉 공격
    protected bool IsMelee
    {
        get
        {
            if (Data.AttackRange < 0f)
            {
                return true;
            }
            return false;
        }
    }

    // 스포너가 스폰 직후 본진 좌표를 넘긴다(주입 방식)
    public void SetTarget(Vector2 target)
    {
        _targetPosition = target;
        _hasTarget = true;
    }

    public override void _Ready()
    {
        Health = Data.MaxHealth;
        if (_sprite != null && Data.Sprite != null)
        {
            _sprite.Texture = Data.Sprite;
        }

        // 스포너가 타깃을 안 넘겼으면 본진을 직접 찾는다(단독 실행·테스트용 폴백)
        if (!_hasTarget)
        {
            Node2D baseNode = GetTree().GetFirstNodeInGroup(HomeBaseGroup) as Node2D;
            if (baseNode != null)
            {
                _targetPosition = baseNode.GlobalPosition;
                _hasTarget = true;
            }
        }

        // 본진 고정이라 방향은 여기서 한 번만 계산 → 이후 직선 유지
        if (_hasTarget)
        {
            _direction = (_targetPosition - GlobalPosition).Normalized();
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        Move(delta);
    }

    // 이동 담당. 기본은 스폰 지점에서 본진으로 직선 이동.
    // 특수 이동 패턴을 가진 몬스터는 이 메서드를 override 한다.
    protected virtual void Move(double delta)
    {
        Velocity = _direction * Data.MoveSpeed;
        MoveAndSlide();
    }

    // 본진 공격. 근/원거리는 Data.AttackRange(IsMelee)로 갈린다.
    // 실제 공격 방식은 서브클래스에서 override 해 구현한다.
    protected virtual void Attack()
    {
    }

    // 피격. 받은 데미지만큼 체력 감소.
    public virtual void TakeDamage(int amount)
    {
        Health -= amount;
        if (Health <= 0)
        {
            Die();
        }
    }

    // 사망 처리. 이펙트·드롭 등이 필요하면 override
    protected virtual void Die()
    {
        QueueFree();
    }
}
