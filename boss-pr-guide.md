# PR: 10웨이브 주사위 리치 보스

## 한 줄 요약

10웨이브에서 등장하는 단독 보스("주사위 리치 마법술사"). 3초마다 주사위를 굴려 7종 패턴 중 하나를 시전하며, 플레이어에 장착된 코어(목줄)를 노린다. 보스를 처치하면 승리.

---

## 컨셉 & 전투 흐름

- 9→10웨이브 진입 시 **코어가 플레이어에 장착**되어 관성(스프링)으로 따라다닌다. 플레이어는 움직여서 코어를 끌고 다니며 회피한다.
- **보스는 우측에 고정**되어 코어를 공격한다(플레이어 HP 없음 — 코어 체력이 곧 체력 풀).
- 보스↔코어 사이에 **상시 빨간 예고선**이 표시된다.
- 보스는 **주기적 기본 공격을 하지 않는다.** 모든 공격/방해는 오직 주사위 결과로만 나간다.

---

## 주사위 패턴 (3초 주기, 1~6 균등 + 7 희귀)

| 눈 | 패턴 | 동작 | 애니 |
|---|---|---|---|
| 1 | 방패병 소환 | 보스 전방(왼쪽)에 방패병 8마리를 세로 벽으로 소환. 물리 오버랩 기반 마법 타겟팅을 몸으로 가로막음 | buff |
| 2 | 지팡이 셔플 | 플레이어 지팡이 3개 각각의 마법 슬롯 순서를 셔플(콤보 방해) | debuff |
| 3 | 탄막 연발 | 코어 방향 부채꼴 탄막을 9연발(발마다 각도 살짝 지터) | attack |
| 4 | 코어 속박 | 코어를 3초간 제자리 고정(목줄 무시 → 회피 무력화) | debuff |
| 5 | 자힐 | 1~(잃은 체력) 사이 랜덤량 회복 | buff |
| 6 | 레이저 스프레이 | 왼쪽 ±45° 내 랜덤 방향으로 12발을 하나씩 순차 발사(발당 짧은 차징) | attack |
| 7 | 조커(플레이어 강화) | 플레이어 공격속도 100배 x 2초 | lucky |

## 상태이상 재해석

보스는 일반 몬스터의 디버프를 자기 식으로 재해석한다.

| 상태이상 | 효과 |
|---|---|
| 화상(Fire) | 그대로 지속 데미지 |
| 슬로우(Ice) | 탄막 발사 속도 감소(`MoveSpeedMultiplier`를 탄속에 곱함) |
| 스턴(Earth) | debuff 애니 재생(과거 레이저 캔슬 대상이던 상시 레이저는 제거됨) |

---

## 아키텍처

역할 3분할: **Boss = 몸통 + 공개 API** / **BossPatternController = 스케줄링** / **IBossPattern = 한 사이클 동작**.

```
class/
  Boss.cs                       Monster 상속. 코어 조준·예고선·목줄·애니 훅·패턴용 공개 API
  Monster.cs                    (공유) UpdateBehavior/OnStunned virtual 훅, protected 접근자 추가
  Spawner.cs                    (공유) BossScene 슬롯 + SpawnBoss 분리
  boss/
    IBossPattern.cs             패턴 계약(Start/Tick/IsFinished)
    BossPatternController.cs    주사위 롤 스케줄링 + 결과 디스패치(캐스트락)
    BossDicePattern.cs          주사위 롤러(가중 랜덤 + 얼굴 플리커)
    BossBarragePattern.cs       탄막 부채꼴 발사 static 유틸(FireFan)
    BossBullet.cs               탄막 투사체(거리 기반 판정, 콜리전 미사용)
    BossDiceShieldPattern.cs    주사위1
    BossDiceShufflePattern.cs   주사위2
    BossDiceBarragePattern.cs   주사위3
    BossDiceRootPattern.cs      주사위4
    BossDiceHealPattern.cs      주사위5
    BossDiceLaserSprayPattern.cs / BossLaserSprayBeam.cs  주사위6 (로직 + 시각 노드)
    BossDicePlayerBoostPattern.cs 주사위7
    BossAnimator.cs             idle 상시 + 단발 애니 자동 복귀(die는 MonsterAnimator가 담당)
class_resource/
  BossData.cs                   보스 전 파라미터(MonsterData 상속). 밸런싱은 전부 데이터로
Scripts/
  Core.cs                       (공유) 목줄(StartLeash/스프링 추적) + 속박(SetRooted) 추가
  RoundManager.cs               (공유) 10라운드를 보스 단독 웨이브로 교체
```

### 설계 결정 메모

- **Boss는 Monster 상속**: 플레이어 마법(`MagicNode`)이 `List<Monster>`를 타겟으로 모으기 때문에, 비-Monster 보스는 피격 판정을 못 받음.
- **판정 방식이 패턴별로 다름**:
  - 탄막(3): 실제 날아가는 투사체(`BossBullet`)가 매 프레임 코어와 거리 체크. 콜리전 레이어 추가 없이 거리 판정.
  - 레이저 스프레이(6): 발사 순간 점(코어)-선분(레이저) 최단거리 1회 계산(순수 기하, 콜리전 아님). 랜덤 방향이라 맞을 수도/빗나갈 수도 있음(의도).
- **방패병 벽**: `MagicNode` 타겟팅이 거리순이 아니라 Area2D 물리 오버랩 순서라, 벽으로 세워두면 별도 로직 없이 마법을 가로막음.
- **웨이브10 진입 신호 없음**: 보스 스폰 자체가 신호. `Boss._Ready()`에서 코어에 목줄을 걸고, 기존 "몬스터 전멸 감지" 로직이 보스 처치를 그대로 클리어로 인식.

---

## ⚠️ 에디터 세팅 필수 (머지 후 인스펙터에서 배선)

코드만으로는 안 돌아가는 부분. 씬에서 아래를 채워야 함:

1. **`battle_world.tscn` → Spawner 노드**: 새 `BossScene` 슬롯에 `res://Scenes/boss.tscn` 할당.
   - **안 하면 조용히 실패**: 보스가 깡통 `Monster`로 폴백돼 패턴/애니/목줄 전부 안 돎.
2. **`boss.tscn` → Boss 노드** export 확인(대부분 이미 배선됨):
   - `BulletScene` = boss_bullet.tscn
   - `MonsterScene` = monster.tscn (방패병 소환용)
   - `ShieldData` = skeleton_boss_shield.tres (MoveSpeed=0, 원거리형, 데미지 0)
   - `DiceSprite`, `_animatedSprite` = 각 AnimatedSprite2D 노드
   - `Data` = boss_data.tres
3. **`boss_data.tres`**: 밸런스 수치는 전부 여기 인스펙터에서 조정(체력, 주사위 가중치, 각 패턴 파라미터).

---

## 테스트 방법

### A. 독립 씬 (`Scenes/test_bosscombat.tscn`)
Core + Player + Boss만 있는 격리 씬. Main/BattleWorld 없이 패턴을 빠르게 검증.
- 주사위 굴림/탄막/레이저/방패병/자힐/속박/목줄 동작 확인 가능.
- **주사위 2(셔플)·7(공속버프)는 이 씬에서 스킵됨** — WandManager/Blackboard.Main이 없어서 `[Dice2/7] ... 스킵` 로그만 출력. 통합 씬에서 확인해야 함.

### B. 통합 (10라운드 진입)
`battle_world.tscn`에서 실제로 10라운드까지 진행하거나, `RoundManager.RoundNumber`를 10으로 세팅해 바로 보스전 진입.
- 코어 목줄, 셔플, 공속버프까지 전부 확인.

### 로그로 확인 가능한 이벤트
`[Dice] 굴림 시작` → `[Dice] N` → 각 패턴 로그(`[Dice1] 방패병...`, `[Barrage] 발사`, `[Dice6] N/12` 등).

---

## 알려진 이슈 / TODO

- **`Boss.SummonShield`의 `GetParent()` 폴백**: 독립 씬(`Blackboard.Main` null 시 EntityContainer 접근이 NRE)을 위한 임시 우회. 통합 검증 후 `Blackboard.EntityContainer`만 쓰도록 롤백 예정(코드에 TODO 주석 있음).
- **승리/등장 연출 없음**: 현재는 처치 시 로그만. 연출은 후속 작업.
- **이펙트 애니메이션 미적용**: 탄막/레이저 자체 비주얼 이펙트는 후속.

---

## 리뷰어 참고

- **`Monster.cs` diff가 크게 보이는 이유**: 파일 전체가 스페이스→탭 재들여쓰기됨. **실제 로직 변경은 3가지뿐** — ① `UpdateBehavior(delta)` virtual 훅 추출, ② `OnStunned(delta)` virtual 훅 추가(스턴 브랜치에서 호출), ③ `HasTarget`/`Core`/`TargetNode`/`AnimatedSprite` protected 접근자 추가. **whitespace 무시하고 보면 diff가 작음** (`git diff -w` 권장).
- **`MonsterData.cs` / `SpawnEntry.cs`**: 내용 변경 없음, 들여쓰기(스페이스→탭)만 바뀐 노이즈.
- **공유 파일 터치**: `Monster.cs`, `Spawner.cs`, `RoundManager.cs`, `Core.cs` — 전부 기존 동작 보존(훅/슬롯 추가 방식), 다른 웨이브·몬스터 동작에 영향 없음.
