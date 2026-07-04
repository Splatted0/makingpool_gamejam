using Godot;

public partial class Core : StaticBody2D, IEntity
{
    [Export] public Team Team { get; set; } = Team.Player;

    [Export] public int MaxHp { get; set; } = 100;

    public int Health { get; set; }

    [Signal]
    public delegate void HpChangedEventHandler(int health, int maxHp);

    [Signal]
    public delegate void DiedEventHandler();

    public override void _Ready()
    {
        Health = MaxHp;
        AddToGroup("core");

        // idle 애니 자동 재생(Autoplay 대체). default 애니가 loop라 계속 재생됨.
        GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D")?.Play();

        SyncHp();
    }

    public void Hit(HitInfo hitInfo)
    {
        if (hitInfo.SourceTeam == Team)
            return;

        TakeDamage(hitInfo.Damage);
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0)
            return;

        Health = Mathf.Max(Health - damage, 0);

        SyncHp();

        GD.Print($"Core HP: {Health}/{MaxHp}");

        if (Health <= 0)
        {
            EmitSignal(SignalName.Died);
            GD.Print("Core destroyed");
        }
    }

    private void SyncHp()
    {
        Blackboard.SetHealth(Health, MaxHp);
        EmitSignal(SignalName.HpChanged, Health, MaxHp);
    }
}