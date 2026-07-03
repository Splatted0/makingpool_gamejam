# 코드 작성 파이프라인

## 개요

사용자의 코딩 요청을 분석·계획·작성·검토·적용하는 다단계 에이전트 파이프라인.

## 전체 흐름

```
사용자 요청
  └─▶ code-orchestrator (진입점 · 전체 조율)
          │
          ├─▶ code-reader           → 관련 파일 컨텍스트 수집
          │
          ├─▶ code-writer           → 계획 기반 초안 작성
          │
          ├─▶ code-final-destination     → 코드 품질 검토 · 수정 요청
          │       ├─▶ code-optimizer          (성능 최적화)
          │       ├─▶ design-pattern-reviewer (Godot 패턴)
          │       └─▶ naming-reviewer         (네이밍 규칙)
          │       └──(최대 2회 수정 요청)──▶ code-writer
          │
          └─▶ code-applier          → 최종 코드 파일에 적용
                    └── 결과 요약 ──▶ code-orchestrator ──▶ 사용자
```

## 에이전트 역할 요약

| 에이전트 | 분류 | 역할 |
|---|---|---|
| `code-orchestrator` | orchestrator | 진입점. 전 단계 순차 위임 후 결과 보고 |
| `code-reader` | scanner | 관련 파일 읽기·컨텍스트 추출. 읽기 전용 |
| `code-writer` | writer | 계획서 기반 코드 초안 작성. 수정 요청 수용 |
| `code-final-destination` | orchestrator | 3개 전문가 에이전트 호출, 피드백 종합, 수정 요청 |
| `code-optimizer` | specialist | 성능·GC·Godot 특화 최적화 검토. 읽기 전용 |
| `design-pattern-reviewer` | specialist | Godot 아키텍처·신호·씬 구조 패턴 검토. 읽기 전용 |
| `naming-reviewer` | specialist | C#/GDScript 네이밍 컨벤션 검토. 읽기 전용 |
| `code-applier` | writer | 최종 코드를 실제 파일에 적용 |

## 단계별 입출력

### 1단계 — code-reader
- **입력**: 태스크 설명, 선택적 파일 힌트
- **출력**: `{ files, symbols, dependencies, conventions }`

### 2단계 — code-writer (초안)
- **입력**: code-orchestrator의 계획서 + code-reader 컨텍스트
- **출력**: 코드 초안 (파일별 코드 블록)

### 3단계 — code-final-destination
- **입력**: code-writer 초안
- **출력**: 수정된 코드 또는 승인된 최종본 + 검토 보고서

### 4단계 — code-applier
- **입력**: code-final-destination 승인 코드
- **출력**: 적용 결과 요약 (생성/수정 파일 목록, 라인 수)

## 수정 루프 규칙

- `code-final-destination`은 3개 전문가 피드백을 종합해 **1개의 통합 수정 요청**을 생성한다.
- `code-writer`는 해당 요청을 반영해 수정본을 반환한다.
- 이 루프는 **최대 2회**. 2회 후 최선본을 `code-applier`에 전달한다.
- 수정 요청이 없으면 초안을 바로 `code-applier`로 전달한다.

## 관련 문서

- [project.md](project.md) — 엔진 환경 (Godot 4.7, C#, GDScript)
- [structure.md](structure.md) — .claude/ 전체 구조 지도
