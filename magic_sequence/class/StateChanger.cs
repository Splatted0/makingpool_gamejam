
public partial class StateChanger: Node
{

    public async void Start()
    {
        await MainMemuState();
        await BattleState();
    }

    public async Task MainMemuState()
    {
        Blackboard.MainMenu.Visible = true;
        await ToSignal(Blackboard.MainMenu, MainMenu.SignalName.GameStartPressed);
        Blackboard.MainMenu.Visible = false;
    }
    
    public async Task EnhanceState()
    {
        EnhanceManager enhanceManager = Blackboard.EnhanceManager;
        enhanceManager.Visible = true;
        enhanceManager.Setup();
        await ToSignal(enhanceManager, EnhanceManager.SignalName.EnhanceEnd);
        enhanceManager.Visible = false;
    }

    public async Task BattleState()
    {
        RoundManager roundManager = Blackboard.RoundManager;
        Blackboard.BattleWorldHud.Visible = true;
        await ToSignal(roundManager, RoundManager.SignalName.RoundEnded);
        Blackboard.BattleWorldHud.Visible = false;
    }
}
