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
    [Export] public float DiceShakeMagnitude { get; set; } = 6f;     // 굴러가는 동안 주사위 스프라이트 흔들림 크기(px, 0이면 안 흔들림)
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
    [Export] public float ShieldScale { get; set; } = 1.6f;              // 소환되는 방패병 크기 배율

    // 3: 탄막 패턴을 연속 발사
    [ExportSubgroup("Dice3 BarrageBurst")]
    [Export] public int BarrageBurstCount { get; set; } = 9;
    [Export] public float BarrageBurstGap { get; set; } = 0.3f;  // 연발 사이 간격(초)
    [Export] public float BarrageBurstAngleJitter { get; set; } = 15f;  // 매 연발마다 기준 각도에 더해지는 무작위 오차(도)

    // 5: 자기 회복 — 1~(잃은 체력) 사이 랜덤량. 힐 "+" VFX를 짧은 간격으로 여러 개 다라락 띄운다.
    [ExportSubgroup("Dice5 Heal")]
    [Export] public int HealVfxBurstCount { get; set; } = 5;         // 힐 "+"를 몇 개 띄울지
    [Export] public float HealVfxBurstInterval { get; set; } = 0.09f; // 힐 "+" 사이 간격(초)
    [Export] public float HealVfxSpread { get; set; } = 30f;         // 힐 "+"가 보스 주변에 흩어지는 범위(px)

    // 6: 전방에 랜덤 방향 레이저 여러 발
    [ExportSubgroup("Dice6 LaserSpray")]
    [Export] public int LaserSprayCount { get; set; } = 12;
    [Export] public float LaserSprayArcDegrees { get; set; } = 60f;  // 전방(-X) 기준 랜덤 분포 각도(위아래 각 절반씩)
    [Export] public int LaserSprayDamage { get; set; } = 150;
    [Export] public float LaserSprayLength { get; set; } = 1200f;   // 시작점에서 전방으로 뻗는 레이저 길이
    [Export] public float LaserSprayBackExtension { get; set; } = 2000f;  // 시작점 뒤로도 이만큼 이어서 시작점이 화면 밖으로 나가게 함
    [Export] public float LaserSpawnOffsetX { get; set; } = 150f;   // 보스 기준 오른쪽으로 이만큼 뒤에서 발사(보스 몸통 안 뚫고 나오게)
    [Export] public float LaserSprayChargeTime { get; set; } = 0.8f;   // 발마다 얇은 선행 레이저가 나온 뒤 임팩트까지 걸리는 시간(길수록 얇은 선만으로 압박감)
    [Export] public float LaserSprayShotInterval { get; set; } = 0.08f; // 다음 발의 얇은 선이 나오기까지 간격(다라락 연속 발사)
    [Export] public float LaserSprayFadeDuration { get; set; } = 0.12f;  // 임팩트 후 사라지는 시간(짧을수록 빡 하고 사라짐)
    [Export] public float LaserSprayThinWidth { get; set; } = 3f;      // 선행 얇은 레이저 두께
    [Export] public float LaserSprayImpactWidth { get; set; } = 32f;   // 격발 순간 확 굵어지는 임팩트 레이저 두께
    [Export] public float LaserSprayHitRadius { get; set; } = 24f;     // 실제 판정 폭(선 기준 좌우로 이 거리, 즉 총 폭은 이 값의 2배). 연출 두께와 별개로 독립 조절

    // 4: 코어를 잠깐 속박(목줄 무시, 제자리 고정)해 회피 이동을 무력화
    [ExportSubgroup("Dice4 CoreRoot")]
    [Export] public float CoreRootDuration { get; set; } = 3f;
    [Export] public float RootChainWidth { get; set; } = 48f;   // 화면 모서리→코어 사슬 두께(Tile 모드라 두께를 키우면 무늬 한 칸 길이도 같이 커짐)

    // 7: 재발사 대기시간을 잠깐 거의 없애서 연타하면 연사되게(반전 조커)
    [ExportSubgroup("Dice7 PlayerBoost")]
    [Export] public float PlayerCooldownMultiplier { get; set; } = 0.05f;  // 재발사 대기시간에 곱해질 배율(작을수록 빠름)
    [Export] public float PlayerRapidFireDuration { get; set; } = 3f;      // 지속시간(초)

    // === 기본 공격: 주사위와 무관하게 일정 주기로 플레이어 근방에 장판(마법진)을 동시에 깐다 ===
    // 발밑 가까이(Near) GroundZoneNearCount개 + 비교적 멀리(Far) 나머지로 나눠서 배치한다.
    [ExportGroup("GroundZone")]
    [Export] public float GroundZoneInterval { get; set; } = 3f;          // 발동 주기(초)
    [Export] public int GroundZoneCount { get; set; } = 9;                // 한 번에 까는 장판 총 개수
    [Export] public int GroundZoneNearCount { get; set; } = 3;            // 이 중 발밑 가까이 깔릴 개수(나머지는 멀리)
    [Export] public float GroundZoneRadius { get; set; } = 95f;           // 장판 판정 반지름(px)
    [Export] public int GroundZoneDamage { get; set; } = 20;
    [Export] public float GroundZoneTelegraphDuration { get; set; } = 1f;   // 예고 시간
    [Export] public float GroundZoneActiveDuration { get; set; } = 0.5f;    // 활성화 후 페이드아웃 시간
    [Export] public float GroundZoneNearRange { get; set; } = 140f;       // 발밑 장판: 플레이어에서 이 거리 안에 배치
    [Export] public float GroundZoneFarInner { get; set; } = 260f;        // 멀리 장판: 이 거리 바깥에 배치(시작 반경)
    [Export] public float GroundZoneSpreadRange { get; set; } = 360f;     // 멀리 장판 최대 배치 반경
    [Export] public float GroundZoneMinSpacing { get; set; } = 100f;      // 장판끼리 최소 이 거리(반지름 2배보다 작게 잡아 살짝 겹치는 건 허용)

    // === 보스 웨이브 중 플레이어 이동 제한: 보스 기준 왼쪽으로 이 거리 지점까지만 접근 허용(위아래 무제한) ===
    [ExportGroup("PlayerLimit")]
    [Export] public float PlayerMoveLimitOffset { get; set; } = 200f;
}
