using Godot;

public partial class InhanceManager : Node
{
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
    }

    public void Setup()
    {
        ExitButton.Visible = false;

        Magic[] magics = DropUtil.GetMagicDrops(Blackboard.MagicPool, 2);
        Wand[] wands = DropUtil.GetWandDrops(Blackboard.WandPool, 1);

        MagicChanceManager.Setup(magics);

        _getWand = wands[0];
        GetWandUi.Setup(_getWand);
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
        MagicChanceManager.Setup(new Magic[0]);
        ExitButton.Visible = true;
    }

    private void OnExitButtonPressed()
    {
    }
}
