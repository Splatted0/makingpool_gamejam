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

        await Tutorial.PlaySteps(tutorial, new[]
        {
            new Tutorial.Step("???", "주인공, 일어나세요. 시간이 많지 않습니다.", "res://texture/magicIcon/fire1.png"),
            new Tutorial.Step("???", "리치 레이가 곧 쳐들어올 겁니다. 전투 능력이 깨어진 지금은 마법의 흐름부터 다시 익혀야 합니다.", "res://texture/magicIcon/ground1.png"),
            new Tutorial.Step("주인공", "제가 리치를 물리치게요. 그런데 마법은 어떻게 쓰죠?", "res://texture/magicIcon/ice1.png"),
            new Tutorial.Step("???", "마력은 굳건하지만 아직 제어가 불안정하군요. 지팡이에 마법을 실어 자동으로 순환시키면 됩니다.", "res://texture/magicIcon/wind1.png"),
        });

        battleWorldHud.Visible = true;
        SetupTutorialEnemy(battleWorldHud);

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

        await Tutorial.ShowMessage(tutorial, "???", "기본 지팡이를 준비했습니다. 먼저 불꽃탄을 장착해 보겠습니다.", "res://texture/magicIcon/fire1.png");
        beginnerWand.Add(fireBullet, 0);
        battleWorldHud.WandManager?.SetupWands();

        await ToSignal(GetTree().CreateTimer(1.0), SceneTreeTimer.SignalName.Timeout);

        await Tutorial.ShowMessage(tutorial, "주인공", "아, 이렇게 자동으로 발사되는군요!", "res://texture/magicIcon/fire1.png");
        beginnerWand.Add(rockSpike, 1);
        beginnerWand.Add(windArrow, 2);
        battleWorldHud.WandManager?.SetupWands();

        await Tutorial.ShowMessage(tutorial, "???", "마법은 지팡이 슬롯 순서대로 순환합니다. 서로 다른 원소를 조합하면 추가 효과도 생깁니다.", "res://texture/magicIcon/wind1.png");
        await Tutorial.ShowMessage(tutorial, "???", "강화에서 얻은 마법은 더 강해질 수 있습니다. 이제 실제 전투로 넘어가죠.", "res://texture/magicIcon/ground1.png");

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
