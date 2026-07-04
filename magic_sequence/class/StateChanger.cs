public partial class StateChanger : Node
{
    public async void Start()
    {
        while (true)
        {
            await MainMemuState();

            bool gameOver = false;
            await EnhanceState();

            while (!gameOver)
            {
                gameOver = await BattleState();

                if (!gameOver)
                    await EnhanceState();
            }
        }
    }

    public async Task MainMemuState()
    {
        Blackboard.MainMenu.Visible = true;
        await ToSignal(Blackboard.MainMenu, MainMenu.SignalName.GameStartPressed);
        Blackboard.MainMenu.Visible = false;
        Blackboard.BattleWorldHud.Visible = true;
    }

    public async Task EnhanceState()
    {
        EnhanceManager enhanceManager = Blackboard.EnhanceManager;
        enhanceManager.Visible = true;
        enhanceManager.Setup();
        await ToSignal(enhanceManager, EnhanceManager.SignalName.EnhanceEnd);
        enhanceManager.Visible = false;
    }

    public async Task<bool> BattleState()
    {
        RoundManager roundManager = Blackboard.RoundManager;
        BattleWorldHud battleWorldHud = Blackboard.BattleWorldHud;
        Core core = battleWorldHud.GetNodeOrNull<Core>("BattleCenter/Core");
        WandManager wandManager = battleWorldHud.GetNodeOrNull<WandManager>("BattleCenter/WandManager");

        battleWorldHud.Visible = true;
        wandManager?.SetupWands();
        roundManager.StartRound();

        Task roundEnded = WaitForSignal(roundManager, RoundManager.SignalName.RoundEnded);
        Task coreDied = core != null
            ? WaitForSignal(core, Core.SignalName.Died)
            : Task.Delay(-1);

        Task completed = await Task.WhenAny(roundEnded, coreDied);

        if (completed == coreDied)
        {
            roundManager.CancelRound();
            await ToSignal(GetTree().CreateTimer(1.0), SceneTreeTimer.SignalName.Timeout);
            return true;
        }
        return false;
    }

    private async Task WaitForSignal(GodotObject source, StringName signal)
    {
        await ToSignal(source, signal);
    }
}
