using Godot;

public partial class EnhanceManager : CanvasLayer
{
    [Signal] public delegate void EnhanceEndEventHandler();
    [Signal] public delegate void InhanceFinishedEventHandler();
    
    [Export] public MagicChangeManager MagicChanceManager;
    [Export] public WandUi GetWandUi;
    [Export] public BaseButton ExitButton;
    [Export] public BaseButton GetWandButton;

    private Wand _getWand;

    public override void _Ready()
    {
        ExitButton.Pressed += OnExitButtonPressed;
        GetWandButton.Pressed += OnGetWandButtonPressed;
        MagicChanceManager.MagicChangeEnd += OnMagicChangeEnd;
        Setup();
    }

    public void Setup()
    {
        ExitButton.Visible = false;


        Magic[] magics = DropUtil.GetMagicDrops(Blackboard.MagicPool, 2);
        MagicChanceManager.Setup(magics);
        
        if (Blackboard.Wands.Length >= 3) {return;}
        Wand[] wands = DropUtil.GetWandDrops(Blackboard.WandPool, 1);
        _getWand = wands[0].Duplicate() as Wand;
        GetWandUi.Setup(_getWand);
        GetWandUi.Visible = true;
        GetWandButton.Visible = true;
    }

    private void OnMagicChangeEnd()
    {
        if (!MagicChanceManager.IsGetMagicRemain)
            ExitButton.Visible = true;
    }

    private void OnGetWandButtonPressed()
    {
        Blackboard.Main.AddWand(_getWand);
        GetWandUi.Visible = false;
        GetWandButton.Visible = false;
        MagicChanceManager.Setup(new Magic[0]);
        ExitButton.Visible = true;
    }

    private void OnExitButtonPressed()
    {
        EmitSignal(SignalName.InhanceFinished);
        EmitSignal(SignalName.EnhanceEnd);
    }
}
