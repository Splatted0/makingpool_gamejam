// 보스 전용 데이터. MonsterData(체력·Frames·골드 등)를 상속하고,
// 보스의 주 패턴·주사위 패턴 수치를 전부 여기에 [Export]로 몰아넣어 밸런싱을 데이터로만 한다.
// 탄환/레이저 프리팹(PackedScene)은 여기 두지 않고 Boss 노드 쪽 [Export]로 둔다(수치 vs 씬 에셋 분리).
[GlobalClass]
public partial class BossData : MonsterData
{
    // === 레이저: 상시 예고선(보스↔코어) → 차지(굵어짐) → 직격 ===
    [ExportGroup("Laser")]
    [Export] public float LaserInterval { get; set; } = 5f;        // 발사 주기(초)
    [Export] public float LaserChargeTime { get; set; } = 1.5f;    // 예고선이 굵어지며 차징하는 시간
    [Export] public int LaserDamage { get; set; } = 300;
    [Export] public bool LaserLockAtChargeStart { get; set; } = false;  // true: 차지 시작 위치 고정, false: 발사 순간까지 코어 추적
    [Export] public float LaserWidthIdle { get; set; } = 2f;       // 평상시 예고선 두께
    [Export] public float LaserWidthMax { get; set; } = 16f;       // 발사 직전(차지 완료) 두께

    // === 탄막: 코어 방향 부채꼴 확산 ===
    [ExportGroup("Barrage")]
    [Export] public float BarrageInterval { get; set; } = 4f;      // 발사 주기(초)
    [Export] public int BarrageBulletCount { get; set; } = 7;
    [Export] public float BarrageSpreadDegrees { get; set; } = 60f; // 부채꼴 전체 각도
    [Export] public float BarrageBulletSpeed { get; set; } = 220f;  // 슬로우 상태이상 시 이 값에 배율이 곱해짐
    [Export] public int BarrageDamage { get; set; } = 50;

    // === 주사위: 3초마다 굴려 방해 패턴 시전(노데미지). 1~6 균등, 7만 낮게 ===
    [ExportGroup("Dice")]
    [Export] public float DiceInterval { get; set; } = 3f;
    [Export] public float DiceWeight1 { get; set; } = 1f;   // 방패병 소환
    [Export] public float DiceWeight2 { get; set; } = 1f;   // 지팡이 셔플
    [Export] public float DiceWeight3 { get; set; } = 1f;   // 탄막 3연발
    [Export] public float DiceWeight4 { get; set; } = 1f;   // 캐릭터 슬로우
    [Export] public float DiceWeight5 { get; set; } = 1f;   // 자힐
    [Export] public float DiceWeight6 { get; set; } = 1f;   // 레이저 6발
    [Export] public float DiceWeight7 { get; set; } = 0.15f; // 캐릭터 도움(반전 조커)

    // 1: 전방에 방패병 소환
    [ExportSubgroup("Dice1 Shield")]
    [Export] public int ShieldSummonCount { get; set; } = 3;

    // 3: 탄막 패턴을 연속 발사
    [ExportSubgroup("Dice3 BarrageBurst")]
    [Export] public int BarrageBurstCount { get; set; } = 3;
    [Export] public float BarrageBurstGap { get; set; } = 0.3f;  // 연발 사이 간격(초)

    // 5: 자기 회복(랜덤량)
    [ExportSubgroup("Dice5 SelfHeal")]
    [Export] public int SelfHealMin { get; set; } = 100;
    [Export] public int SelfHealMax { get; set; } = 400;

    // 6: 전방에 랜덤 방향 레이저 여러 발
    [ExportSubgroup("Dice6 LaserSpray")]
    [Export] public int LaserSprayCount { get; set; } = 6;
    [Export] public float LaserSprayArcDegrees { get; set; } = 90f;  // 전방 랜덤 분포 각도
    [Export] public int LaserSprayDamage { get; set; } = 150;

    // 4: 플레이어 이동속도 감소 (Player 코드 수정 동반 → 맨 마지막 구현)
    [ExportSubgroup("Dice4 PlayerSlow")]
    [Export] public float PlayerSlowMultiplier { get; set; } = 0.4f;
    [Export] public float PlayerSlowDuration { get; set; } = 3f;

    // 7: 플레이어 공격속도 급증(반전 조커)
    [ExportSubgroup("Dice7 PlayerBoost")]
    [Export] public float PlayerAttackSpeedMultiplier { get; set; } = 100f;
    [Export] public float PlayerBoostDuration { get; set; } = 2f;
}
