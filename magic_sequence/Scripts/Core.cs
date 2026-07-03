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

        EmitSignal(SignalName.HpChanged, Health, MaxHp);
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
        EmitSignal(SignalName.HpChanged, Health, MaxHp);

        GD.Print($"Core HP: {Health}/{MaxHp}");

        if (Health <= 0)
        {
            EmitSignal(SignalName.Died);
            GD.Print("Core destroyed");
        }
    }
}