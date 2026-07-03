// 웨이브 구성 한 줄: 어떤 몬스터를 몇 마리 소환할지.
[GlobalClass]
public partial class SpawnEntry : Resource
{
    [Export] public MonsterData Data { get; set; }   // 소환할 몬스터 종류
    [Export] public int Count { get; set; } = 1;     // 마리 수
}
