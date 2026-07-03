// 몬스터 종류별 스탯·외형 템플릿. .tres로 저장해 Monster에 갈아끼운다.
// 스탯만 다른 종류는 이 리소스만 새로 만들면 되고 코드는 건드릴 필요 없다.
[GlobalClass]
public partial class MonsterData : Resource
{
	[Export] public int MaxHealth { get; set; } = 3;
	[Export] public float MoveSpeed { get; set; } = 100f;
	[Export] public int AttackDamage { get; set; } = 1;      // 본진에 주는 데미지
	[Export] public float AttackRange { get; set; } = -1f;   // 음수=근거리(접촉), 0 이상=원거리 사거리
	[Export] public float AttackInterval { get; set; } = 1f; // 공격 간격(초, 공격속도)
	[Export] public SpriteFrames Frames { get; set; }        // 종류별 외형·애니메이션
}
