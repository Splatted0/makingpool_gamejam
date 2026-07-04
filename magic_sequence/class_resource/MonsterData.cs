[GlobalClass]
public partial class MonsterData : Resource
{
	[Export] public int MaxHealth { get; set; } = 3;
	[Export] public float MoveSpeed { get; set; } = 100f;
	[Export] public int AttackDamage { get; set; } = 1;
	[Export] public float AttackRange { get; set; } = -1f;
	[Export] public float AttackInterval { get; set; } = 1f;
	[Export] public int HealAmount { get; set; } = -1;
	[Export] public bool SpawnFront { get; set; } = false;
	[Export] public SpriteFrames Frames { get; set; }
	[Export] public int GoldReward { get; set; } = 1;
}
