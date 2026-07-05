public partial class StateChanger : Node
{
    public async void Start()
    {
        while (true)
        {
            await MainMemuState();

            await TutorialState();

            bool gameOver = false;
            bool gameClear = false;
            await EnhanceState();

            while (!gameOver && !gameClear)
            {
                gameOver = await BattleState();
                gameClear = IsAllRoundsCleared();

                if (!gameOver && !gameClear)
                    await EnhanceState();
            }

            if (gameClear)
            {
                await EndingState();
                return;
            }
            else
            {
                await GameOverState();
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
        Tutorial tutorial = Blackboard.Tutorial;
        BattleWorldHud battleWorldHud = Blackboard.BattleWorldHud;
        tutorial.Visible = true;
        battleWorldHud.Visible = false;
        await tutorial.ShowCutscene("res://texture/cutscenes/first.png");
        
        await tutorial.ShowCutscene("res://texture/cutscenes/1.png");
        await tutorial.ShowDialogue("주인공", "“불 마법을 쓰려는데 얼음 마법이 나오고, 바람 마법을 쓰려는데 불 마법이 나오고!\n도저히 못 살겠어요! 혹시 해결법을 아시나요?” ");

        await tutorial.ShowCutscene("res://texture/cutscenes/6.png");
        await tutorial.ShowDialogue("현자", "“이건 마법이 랜덤하게 나오게되는 가챠의 저주로군. \n이 저주를 가진 이들은 운빨망법사라해서, 강한 마력을 가지고 있지만 \n운에 따라 전투력이 천차만별이 되지. 해결해줄 수야 있다만 리치 놈이 곧 쳐들어올 거야. \n난 전투능력이 떨어지니 대피해야만 하네. “");

        await tutorial.ShowCutscene("res://texture/cutscenes/5.png");
        await tutorial.ShowDialogue("", "주인공  :  “제가 리치를 물리칠게요! 그럼 어떤가요?” \n현자  :  “마력은 굉장하다만... 마법도 제어 못하는 자네가? 할 수 있겠는가?”\n");

        await tutorial.ShowCutscene("res://texture/cutscenes/fifth.png");
        await tutorial.ShowDialogue("", "\n주인공  :  “맡겨만 주세요!”\n현자  :  “흠, 그럼 이런건 어떤가?”\n");

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

        await Blackboard.Tutorial.ShowText("현자", "시전한 마법을 저장할 수 있는 완드라네. 이거라면 불확실성을 줄일 수 있겠지. 자, 마법을 사용해 보게.");

        ShowBattleDemoView();
        beginnerWand.Add(fireBullet, 0);
        tutorialWandManager?.SetupWands();
        tutorialWandManager?.FireOnce(0);
        await ToSignal(GetTree().CreateTimer(1.0), SceneTreeTimer.SignalName.Timeout);
        tutorialWandManager?.FireOnce(0);
        await ToSignal(GetTree().CreateTimer(0.4), SceneTreeTimer.SignalName.Timeout);
        tutorial.Visible = true;

        await Blackboard.Tutorial.ShowText("주인공", "와! 이거라면!");

        ShowBattleDemoView();
        beginnerWand.Add(rockSpike, 1);
        beginnerWand.Add(windArrow, 2);
        tutorialWandManager?.SetupWands();
        tutorialWandManager?.FireOnce(0);
        await ToSignal(GetTree().CreateTimer(1.4), SceneTreeTimer.SignalName.Timeout);
        tutorial.Visible = true;

        await Blackboard.Tutorial.ShowText("현자", "마법학도라면 원소 간의 상성과 역상성에 대해서도 알고 있겠지? 마법을 나눠담을 지팡이를 가져올 테니 일단 리치의 병사들을 막아내고 있게.\n골드를 통해 저장된 마법 자체도 강화할 수 있다네. 그럼 이따 보지!");


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

        // 모든 라운드(보스 포함) 클리어 상태에서 StartRound를 부르면 RoundEnded가 영원히 안 와 소프트락.
        // 승리 연출이 생기기 전까지는 게임 종료로 처리해 메인 메뉴로 복귀시킨다.
        if (roundManager.MaxRounds > 0 && roundManager.RoundNumber > roundManager.MaxRounds)
        {
            GD.Print("[StateChanger] 전 라운드 클리어 — 승리");
            return true;
        }

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

    private static bool IsAllRoundsCleared()
    {
        RoundManager roundManager = Blackboard.RoundManager;
        return roundManager != null
            && roundManager.MaxRounds > 0
            && roundManager.RoundNumber > roundManager.MaxRounds;
    }

    private async Task EndingState()
    {
        Blackboard.BattleWorldHud.Visible = false;
        Blackboard.EnhanceManager.Visible = false;
        Blackboard.MainMenu.Visible = false;

        Tutorial tutorial = Blackboard.Tutorial;
        if (tutorial == null)
            return;

        tutorial.Visible = true;
        await tutorial.ShowCutscene("res://texture/cutscenes/ending0.png");
        await tutorial.ShowDialogue("", "주인공  :  “현자닙, 리치를 물리쳤어요!” \n현자  :  “고맙네! 약속대로 자네의 저주를 해결해주지!”\n");
        await tutorial.ShowCutscene("res://texture/cutscenes/ending1.png");
        await tutorial.ShowDialogue("주인공", "…괜찮아요! 이번에 리치와 싸우면서 느꼈어요. ");
        await tutorial.ShowCutscene("res://texture/cutscenes/ending2.png");
        await tutorial.ShowDialogue("주인공", "운빨망법사도 나쁘지 않다는 걸요!");
    }

    private async Task GameOverState()
    {
        Blackboard.BattleWorldHud.Visible = false;
        Blackboard.EnhanceManager.Visible = false;
        Blackboard.Tutorial.Visible = false;
        Blackboard.MainMenu.Visible = false;

        CanvasLayer layer = new CanvasLayer { Name = "GameOverScreen" };
        AddChild(layer);

        TextureRect image = new TextureRect();
        image.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        image.Texture = GD.Load<Texture2D>("res://Sprites/gameover.png");
        image.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
        image.StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered;
        layer.AddChild(image);

        await WaitForAdvance();
        GetTree().Quit();
    }

    private async Task WaitForAdvance()
    {
        while (IsAdvancePressed())
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        while (!IsAdvancePressed())
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        while (IsAdvancePressed())
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
    }

    private static bool IsAdvancePressed()
    {
        return Input.IsMouseButtonPressed(MouseButton.Left)
            || Input.IsKeyPressed(Key.Space)
            || Input.IsKeyPressed(Key.Enter);
    }
}
