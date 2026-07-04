// 보스 전용 데이터. MonsterData(체력·Frames·골드 등)를 상속하고,
// 보스의 주 패턴·주사위 패턴 수치를 전부 여기에 [Export]로 몰아넣어 밸런싱을 데이터로만 한다.
// 탄환/레이저 프리팹(PackedScene)은 여기 두지 않고 Boss 노드 쪽 [Export]로 둔다(수치 vs 씬 에셋 분리).
[GlobalClass]
public partial class BossData : MonsterData
{
    // === 예고선: 보스↔코어 상시 연결선(항상 idle 두께로 표시, 더 이상 자체적으로 차지·발사하지 않음) ===
    [ExportGroup("Laser")]
    [Export] public float LaserWidthIdle { get; set; } = 2f;       // 평상시 예고선 두께

    // === 탄막: 코어 방향 부채꼴 확산(주사위3에서만 발사) ===
    [ExportGroup("Barrage")]
    [Export] public int BarrageBulletCount { get; set; } = 7;
    [Export] public float BarrageSpreadDegrees { get; set; } = 60f; // 부채꼴 전체 각도
    [Export] public float BarrageBulletSpeed { get; set; } = 220f;  // 슬로우 상태이상 시 이 값에 배율이 곱해짐
    [Export] public int BarrageDamage { get; set; } = 50;

    // === 주사위: 3초마다 굴려 방해 패턴 시전(노데미지). 1~6 균등, 7만 낮게 ===
    [ExportGroup("Dice")]
    [Export] public float DiceInterval { get; set; } = 3f;
    [Export] public float DiceRollDuration { get; set; } = 0.8f;    // 얼굴이 무작위로 바뀌며 굴러가는 시간
    [Export] public float DiceFlickerInterval { get; set; } = 0.05f; // 굴러가는 동안 얼굴이 바뀌는 간격
    [Export] public float DiceWeight1 { get; set; } = 1f;   // 방패병 소환
    [Export] public float DiceWeight2 { get; set; } = 1f;   // 지팡이 셔플
    [Export] public float DiceWeight3 { get; set; } = 1f;   // 탄막 3연발
    [Export] public float DiceWeight4 { get; set; } = 1f;   // 캐릭터 슬로우
    [Export] public float DiceWeight5 { get; set; } = 1f;   // 자힐
    [Export] public float DiceWeight6 { get; set; } = 1f;   // 레이저 6발
    [Export] public float DiceWeight7 { get; set; } = 0.15f; // 캐릭터 도움(반전 조커)

    // 1: 전방(왼쪽)에 방패병을 세로 한 줄로 소환해 벽을 세운다
    [ExportSubgroup("Dice1 Shield")]
    [Export] public int ShieldSummonCount { get; set; } = 8;
    [Export] public float ShieldSummonSpacing { get; set; } = 60f;       // 방패병 사이 세로 간격
    [Export] public float ShieldSummonForwardOffset { get; set; } = 200f; // 보스 기준 전방(왼쪽) 거리

    // 3: 탄막 패턴을 연속 발사
    [ExportSubgroup("Dice3 BarrageBurst")]
    [Export] public int BarrageBurstCount { get; set; } = 9;
    [Export] public float BarrageBurstGap { get; set; } = 0.3f;  // 연발 사이 간격(초)
    [Export] public float BarrageBurstAngleJitter { get; set; } = 15f;  // 매 연발마다 기준 각도에 더해지는 무작위 오차(도)

    // 5: 자기 회복 — 1~(잃은 체력) 사이 랜덤량. 데이터 파라미터 없음(현재 체력에 따라 결정).

    // 6: 전방에 랜덤 방향 레이저 여러 발
    [ExportSubgroup("Dice6 LaserSpray")]
    [Export] public int LaserSprayCount { get; set; } = 12;
    [Export] public float LaserSprayArcDegrees { get; set; } = 90f;  // 전방(-X) 기준 랜덤 분포 각도
    [Export] public int LaserSprayDamage { get; set; } = 150;
    [Export] public float LaserSprayLength { get; set; } = 1200f;   // 레이저 길이
    [Export] public float LaserSprayChargeTime { get; set; } = 0.15f;  // 발당 짧은 전조(가늘게→굵게) 시간
    [Export] public float LaserSprayFadeDuration { get; set; } = 0.25f;  // 발사 후 사라지는 시간

    // 4: 코어를 잠깐 속박(목줄 무시, 제자리 고정)해 회피 이동을 무력화
    [ExportSubgroup("Dice4 CoreRoot")]
    [Export] public float CoreRootDuration { get; set; } = 3f;

    // 7: 재발사 대기시간을 잠깐 거의 없애서 연타하면 연사되게(반전 조커)
    [ExportSubgroup("Dice7 PlayerBoost")]
    [Export] public float PlayerCooldownMultiplier { get; set; } = 0.05f;  // 재발사 대기시간에 곱해질 배율(작을수록 빠름)
    [Export] public float PlayerRapidFireDuration { get; set; } = 3f;      // 지속시간(초)
}
