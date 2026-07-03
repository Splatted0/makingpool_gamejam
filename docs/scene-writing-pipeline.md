# 씬 작성 파이프라인

## 개요

사용자의 씬 생성 요청을 탐색·설계·작성·검토·적용하는 다단계 에이전트 파이프라인.
`.tscn` 파일 생성에 특화되며 코드 파이프라인과 독립적으로 운영된다.

## 전체 흐름

```
사용자 요청
  └─▶ scene-orchestrator (진입점 · 전체 조율)
          │
          ├─▶ scene-reader           → 관련 씬·스크립트·리소스 컨텍스트 수집
          │
          ├─▶ scene-designer         → 씬 파일 목록 및 최상위 노드 구조 설계
          │
          ├─▶ scene-writer           → 세부 노드 트리·속성·인스펙터 값 설계
          │
          ├─▶ scene-final-destination → 씬 품질 검토 · 수정 요청
          │       ├─▶ node-data-reviewer       (노드·데이터 적합성)
          │       ├─▶ tree-structure-reviewer   (트리 구조 적합성)
          │       └─▶ node-naming-reviewer     (노드 네이밍 적합성)
          │       └──(최대 2회 수정 요청)──▶ scene-writer
          │
          └─▶ scene-applier          → 노드 트리 기술서를 실제 .tscn 파일로 변환·저장
                    └── 결과 요약 ──▶ scene-orchestrator ──▶ 사용자
```

## 에이전트 역할 요약

| 에이전트 | 분류 | 역할 |
|---|---|---|
| `scene-orchestrator` | orchestrator | 진입점. 전 단계 순차 위임 후 결과 보고 |
| `scene-reader` | scanner | 관련 .tscn·.tres·스크립트 읽기·컨텍스트 추출. 읽기 전용 |
| `scene-designer` | writer | 씬 파일 목록, 루트 노드 타입, 씬 간 참조 구조 설계 |
| `scene-writer` | writer | 세부 노드 트리·속성·인스펙터 값 설계. 수정 요청 수용 |
| `scene-final-destination` | orchestrator | 3개 전문가 에이전트 병렬 호출, 피드백 종합, 수정 요청 |
| `node-data-reviewer` | specialist | 노드 타입·데이터 값 의도 적합성 검토. 읽기 전용 |
| `tree-structure-reviewer` | specialist | 트리 깊이·너비, 서브씬 분리 필요성 검토. 읽기 전용 |
| `node-naming-reviewer` | specialist | 전체 노드명 컨벤션·명확성 검토. 읽기 전용 |
| `scene-applier` | writer | 최종 노드 트리 기술서를 .tscn 파일로 변환·저장 |

## 단계별 입출력

### 1단계 — scene-reader
- **입력**: 태스크 설명, 선택적 파일 힌트
- **출력**: `{ scenes, scripts, resources, instanced_scenes, conventions }`

### 2단계 — scene-designer
- **입력**: scene-orchestrator의 계획서 + scene-reader 컨텍스트
- **출력**: 씬 계획서 (생성할 씬 목록, 루트 노드 타입, 씬 간 인스턴스 관계)

### 3단계 — scene-writer (초안)
- **입력**: scene-designer 씬 계획서 + scene-reader 컨텍스트
- **출력**: 노드 트리 기술서 (씬별 완전한 노드 트리 + 인스펙터 값)

### 4단계 — scene-final-destination
- **입력**: scene-writer 노드 트리 기술서
- **출력**: 수정된 기술서 또는 승인된 최종본 + 검토 보고서

### 5단계 — scene-applier
- **입력**: scene-final-destination 승인 노드 트리 기술서
- **출력**: 생성된 .tscn 파일 경로 목록 및 적용 요약

## 수정 루프 규칙

- `scene-final-destination`은 3개 전문가 피드백을 종합해 **1개의 통합 수정 요청**을 생성한다.
- `scene-writer`는 해당 요청을 반영해 수정본을 반환한다.
- 이 루프는 **최대 2회**. 2회 후 최선본을 `scene-applier`에 전달한다.
- 수정 요청이 없으면 초안을 바로 `scene-applier`로 전달한다.

## 노드 트리 기술서 형식

scene-writer가 출력하고 scene-final-destination이 검토하는 표준 형식.

```
## SCENE: res://Scenes/Player.tscn

### Node Tree
- [CharacterBody2D] Player
  script: res://Scripts/Player.cs
  - [CollisionShape2D] CollisionShape2D
    inspector:
      shape: CapsuleShape2D(radius=16, height=48)
  - [Sprite2D] Sprite
    inspector:
      texture: res://Assets/Sprites/player.png
  - [AnimationPlayer] AnimationPlayer
  - [Camera2D] Camera
    inspector:
      zoom: Vector2(1.5, 1.5)
  - [Area2D] HurtBox
    - [CollisionShape2D] CollisionShape2D

### Instanced Scenes
- [PlayerHUD.tscn] HUD  ← res://Scenes/UI/PlayerHUD.tscn

### Connections
- signal: Player::health_depleted → GameManager::_on_player_dead
```

## 관련 문서

- [project.md](project.md) — 엔진 환경 (Godot 4.7, C#, GDScript)
- [structure.md](structure.md) — 에이전트·문서 전체 구조 지도
- [code-writing-pipeline.md](code-writing-pipeline.md) — 코드 작성 파이프라인
