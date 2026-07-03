using Godot;

public partial class Core : StaticBody2D
{
    [Signal]
    public delegate void HpChangedEventHandler(int currentHp, int maxHp);

    [Signal]
    public delegate void DiedEventHandler();

    [Export] public int MaxHp { get; set; } = 100;

    public int CurrentHp { get; private set; }

    public override void _Ready()
    {
        CurrentHp = MaxHp;
        AddToGroup("core");

        EmitSignal(SignalName.HpChanged, CurrentHp, MaxHp);
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0)
            return;

        CurrentHp = Mathf.Max(CurrentHp - damage, 0);
        EmitSignal(SignalName.HpChanged, CurrentHp, MaxHp);

        GD.Print($"Core HP: {CurrentHp}/{MaxHp}");

        if (CurrentHp <= 0)
        {
            EmitSignal(SignalName.Died);
            GD.Print("Core destroyed");
        }
    }
}