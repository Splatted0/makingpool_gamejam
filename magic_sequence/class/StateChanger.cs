public partial class StateChanger : Node
{
    public async void Start()
    {
        while (true)
        {
            await MainMemuState();

            await TutorialState();

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
        Blackboard.Tutorial.Visible = true;
    }

    public async Task TutorialState()
    {
        CanvasLayer tutorial = Blackboard.Tutorial ?? GetNodeOrNull<CanvasLayer>("../Tutorial");
        BattleWorldHud battleWorldHud = Blackboard.BattleWorldHud;

        if (tutorial == null || battleWorldHud == null)
        {
            GD.PrintErr("[StateChanger] TutorialState requires Tutorial and BattleWorldHud nodes.");
            return;
        }

        tutorial.Visible = true;
        battleWorldHud.Visible = false;

        await Tutorial.ShowCutscene(
            tutorial,
            "res://texture/magicIcon/fire1.png",
            "res://texture/magicIcon/ground1.png",
            "res://texture/magicIcon/ice1.png",
            "res://texture/magicIcon/wind1.png");

        battleWorldHud.Visible = true;
        SetupTutorialEnemy(battleWorldHud);
        Wand[] previousWands = Blackboard.Main.Wands ?? Array.Empty<Wand>();
        WandManager tutorialWandManager = battleWorldHud.WandManager;
        bool previousAutoFireEnabled = tutorialWandManager?.AutoFireEnabled ?? true;

        Wand beginnerWand = GD.Load<Wand>("res://resource_ingame/resource_wand/beginner_wand.tres")?.Duplicate(true) as Wand;
        Magic fireBullet = GD.Load<Magic>("res://resource_ingame/resource_magic/fire_bullet.tres");
        Magic rockSpike = GD.Load<Magic>("res://resource_ingame/resource_magic/rock_spike.tres");
        Magic windArrow = GD.Load<Magic>("res://resource_ingame/resource_magic/wind_arrow.tres");

        if (beginnerWand == null || fireBullet == null || rockSpike == null || windArrow == null)
        {
            GD.PrintErr("[StateChanger] Tutorial resources are missing.");
            Tutorial.Hide(tutorial);
            tutorial.Visible = false;
            battleWorldHud.Visible = true;
            return;
        }

        beginnerWand.Setup();
        beginnerWand.Magics.Clear();
        beginnerWand.Magics.Resize(beginnerWand.Slot);
        Blackboard.Main.Wands = new[] { beginnerWand };
        if (tutorialWandManager != null)
            tutorialWandManager.AutoFireEnabled = false;

        await Tutorial.ShowDialogue(tutorial, "현자", "시전한 마법을 저장할 수 있는 완드라네. 이거라면 불확실성을 줄일 수 있겠지. 자, 마법을 사용해 보게.");

        ShowBattleDemoView();
        beginnerWand.Add(fireBullet, 0);
        tutorialWandManager?.SetupWands();
        tutorialWandManager?.FireOnce(0);
        await ToSignal(GetTree().CreateTimer(1.0), SceneTreeTimer.SignalName.Timeout);
        tutorialWandManager?.FireOnce(0);
        await ToSignal(GetTree().CreateTimer(0.4), SceneTreeTimer.SignalName.Timeout);
        tutorial.Visible = true;

        await Tutorial.ShowDialogue(tutorial, "주인공", "와! 이거라면!");

        ShowBattleDemoView();
        beginnerWand.Add(rockSpike, 1);
        beginnerWand.Add(windArrow, 2);
        tutorialWandManager?.SetupWands();
        tutorialWandManager?.FireOnce(0);
        await ToSignal(GetTree().CreateTimer(1.4), SceneTreeTimer.SignalName.Timeout);
        tutorial.Visible = true;

        await Tutorial.ShowDialogue(tutorial, "현자", "마법학도라면 원소 간의 상성과 역상성에 대해서도 알고 있겠지? 마법을 나눠담을 지팡이를 가져올 테니 일단 리치의 병사들을 막아내고 있게. 골드를 통해 저장된 마법 자체도 강화할 수 있다네. 그럼 이따 보지!");
        await Tutorial.ShowCutscene(tutorial, "res://texture/magicIcon/support1.png");

        ClearTutorialBattleObjects(battleWorldHud);
        Blackboard.Main.Wands = previousWands
            .Where(wand => System.IO.Path.GetFileNameWithoutExtension(wand?.ResourcePath ?? "") != "beginner_wand")
            .ToArray();
        if (tutorialWandManager != null)
        {
            tutorialWandManager.AutoFireEnabled = previousAutoFireEnabled;
            tutorialWandManager.SetupWands();
        }
        Tutorial.Hide(tutorial);
        tutorial.Visible = false;
        battleWorldHud.Visible = true;

        void SetupTutorialEnemy(BattleWorldHud hud)
        {
            Node2D container = hud.EntityContainer;
            Core core = hud.Core;
            if (container == null || core == null)
                return;

            foreach (Node child in container.GetChildren())
                child.QueueFree();

            PackedScene monsterScene = GD.Load<PackedScene>("res://Scenes/monster.tscn");
            MonsterData slimeData = GD.Load<MonsterData>("res://resource_ingame/resource_monster/slime.tres")?.Duplicate(true) as MonsterData;
            if (monsterScene == null || slimeData == null)
                return;

            slimeData.MaxHealth = 10000;
            slimeData.MoveSpeed = 0f;
            slimeData.AttackDamage = 0;
            slimeData.GoldReward = 0;

            Monster slime = monsterScene.Instantiate<Monster>();
            slime.Data = slimeData;
            slime.SetTarget(core.GlobalPosition, core);
            slime.GlobalPosition = new Vector2(1080f, 330f);
            container.AddChild(slime);
        }

        void ShowBattleDemoView()
        {
            tutorial.Visible = false;
            battleWorldHud.Visible = true;

            Node2D battleCenter = battleWorldHud.GetNodeOrNull<Node2D>("BattleCenter");
            if (battleCenter != null)
                battleCenter.Visible = true;

            Player player = battleWorldHud.GetNodeOrNull<Player>("BattleCenter/Player");
            if (player != null)
            {
                player.Visible = true;
                player.Modulate = Colors.White;
                player.GlobalPosition = new Vector2(312f, 329f);
                player.ZIndex = 100;
            }

            WandManager wandManager = battleWorldHud.WandManager;
            if (wandManager != null)
            {
                wandManager.AutoFireEnabled = false;
                wandManager.Player ??= player;
                wandManager.Projectiles ??= battleWorldHud.GetNodeOrNull<Node>("BattleCenter/Projectiles");
                wandManager.Core ??= battleWorldHud.Core;
            }
        }

        void ClearTutorialBattleObjects(BattleWorldHud hud)
        {
            if (hud.EntityContainer != null)
            {
                foreach (Node child in hud.EntityContainer.GetChildren())
                    child.QueueFree();
            }

            hud.RoundManager?.ClearProjectiles();
        }
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
