// 몬스터 소환 실행자. WaveData를 받아 타이머로 하나씩 소환한다.
// 웨이브 내용·진행은 결정하지 않음 — 매니저가 SpawnStart로 넘긴 것만 실행.
public partial class Spawner : Node2D
{
	[Signal] public delegate void SpawnFinishedEventHandler();   // 큐+보스 다 소환하면 방출

	[Export] public PackedScene MonsterScene { get; set; }   // Monster.tscn
	[Export] public Area2D SpawnArea { get; set; }           // 랜덤 스폰 영역(사각 CollisionShape2D 가정)
	[Export] public Node2D Container { get; set; }           // 소환한 몬스터를 붙일 부모
	[Export] public Core Core { get; set; }              // 본진 (SetTarget용, Hit 호출을 위해 Core 타입)

	[Export] private WaveData _testWave;                     // 단독 테스트용(매니저 붙으면 제거)
	[Export] public bool AutoStartTestWave { get; set; } = false;

	private Timer _timer;
	private CollisionShape2D _spawnCollision;                // SpawnArea의 사각 콜리전 캐싱
	private Godot.Collections.Array<MonsterData> _spawnList = new();
	private int _spawnIndex;
	private MonsterData _pendingBoss;

	public override void _Ready()
	{
		_timer = new Timer();
		_timer.OneShot = false;
		_timer.Timeout += OnSpawnTick;
		AddChild(_timer);

		_spawnCollision = null;

		if (SpawnArea != null)
		{
			_spawnCollision = FindCollisionShape(SpawnArea);
		}

		if (MonsterScene == null)
		{
			GD.PrintErr($"[Spawner] {Name}: MonsterScene이 비어있습니다.");
		}
		if (SpawnArea == null)
		{
			GD.PrintErr($"[Spawner] {Name}: SpawnArea가 비어있습니다.");
		}
		else if (_spawnCollision == null)
		{
			GD.PrintErr($"[Spawner] {Name}: SpawnArea 밑에 CollisionShape2D가 없습니다.");
		}
		if (Container == null)
		{
			GD.PrintErr($"[Spawner] {Name}: Container가 비어있습니다.");
		}
		if (Core == null)
		{
			GD.PrintErr($"[Spawner] {Name}: Core가 비어있습니다.");
		}

		if (MonsterScene == null || SpawnArea == null || _spawnCollision == null || Container == null || Core == null)
		{
			GD.PrintErr($"[Spawner] {Name}: 필수 참조가 비어 있어서 스폰을 중단합니다.");
			return;
		}

		if (AutoStartTestWave && _testWave != null)
		{
			SpawnStart(_testWave);
		}
	}

	// 웨이브 하나를 받아 소환 시작. 매니저가 호출.
	public void SpawnStart(WaveData wave)
	{
		if (wave == null)
		{
			GD.PrintErr($"[Spawner] {Name}: SpawnStart에 null WaveData가 전달되었습니다.");
			return;
		}

		_spawnList = new Godot.Collections.Array<MonsterData>();
		foreach (SpawnEntry entry in wave.Entries)
		{
			if (entry == null || entry.Data == null)
			{
				continue;
			}
			for (int i = 0; i < entry.Count; i++)
			{
				_spawnList.Add(entry.Data);
			}
		}

		_spawnList.Shuffle();   // 몬스터 소환 순서는 항상 섞는다
		_spawnIndex = 0;
		_pendingBoss = wave.Boss;

		_timer.WaitTime = wave.Interval;
		_timer.Start();

		GD.Print($"[Spawner] {Name}: 스폰 시작 (총 {_spawnList.Count}마리, 보스 {_pendingBoss != null}, interval {wave.Interval})");
	}

	// 타이머 tick마다 한 마리씩 소환. 큐 소진 후 보스, 그 뒤 종료.
	private void OnSpawnTick()
	{
		if (_spawnIndex < _spawnList.Count)
		{
			SpawnMonster(_spawnList[_spawnIndex], RandomPointInArea());
			_spawnIndex++;
		}
		else if (_pendingBoss != null)
		{
			SpawnMonster(_pendingBoss, SpawnAreaCenter());
			_pendingBoss = null;
		}

		// 큐도 보스도 남지 않았으면 종료
		if (_spawnIndex >= _spawnList.Count && _pendingBoss == null)
		{
			_timer.Stop();
			GD.Print($"[Spawner] {Name}: 스폰 끝");
			EmitSignal(SignalName.SpawnFinished);
		}
	}

	// Instantiate 후 AddChild 전에 Data·타깃·위치를 넣는다(Monster._Ready가 그 값을 읽으므로).
	private void SpawnMonster(MonsterData data, Vector2 worldPos)
	{
		Monster monster = MonsterScene.Instantiate<Monster>();

		monster.Data = data;

		if (Core != null)
			monster.SetTarget(Core.GlobalPosition, Core);
		else
			GD.PrintErr($"[Spawner] {Name}: Core가 null이라 monster target을 지정하지 못했습니다.");

		monster.Position = Container.ToLocal(worldPos);
		Container.AddChild(monster);
	}

	// SpawnArea 사각 영역 내 랜덤 좌표(월드).
	private Vector2 RandomPointInArea()
	{
		RectangleShape2D rect = (RectangleShape2D)_spawnCollision.Shape;
		Vector2 size = rect.Size;
		Vector2 center = _spawnCollision.GlobalPosition;
		float x = center.X + (float)GD.RandRange(-size.X * 0.5, size.X * 0.5);
		float y = center.Y + (float)GD.RandRange(-size.Y * 0.5, size.Y * 0.5);
		return new Vector2(x, y);
	}

	// 보스 위치 = 영역 중앙(우측 밴드 X, Y 중앙).
	private Vector2 SpawnAreaCenter()
	{
		return _spawnCollision.GlobalPosition;
	}

	private static CollisionShape2D FindCollisionShape(Area2D area)
	{
		foreach (Node child in area.GetChildren())
		{
			if (child is CollisionShape2D shape)
			{
				return shape;
			}
		}
		return null;
	}
	public WaveData GetDefaultWave()
	{
		return _testWave;
	}
}
