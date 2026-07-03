// 한 웨이브의 소환 데이터. 매니저 등 외부에서 채워 스포너에 넘긴다.
// 스포너는 이 데이터만 받아 타이머 돌리며 실행한다(웨이브 내용은 결정하지 않음).
[GlobalClass]
public partial class WaveData : Resource
{
    [Export] public Godot.Collections.Array<SpawnEntry> Entries { get; set; } = new();   // 소환 큐(종류+갯수)
    [Export] public float Interval { get; set; } = 1f;      // 소환 간격(초)
    [Export] public MonsterData Boss { get; set; }          // 보스(null이면 없음). 큐 소진 후 마지막에 소환
}
