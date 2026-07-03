# 에이전트 · 문서 구조

## 분류 기준

| 분류 | 위치 | 역할 |
|---|---|---|
| command | `agents/commands/` | 사용자가 직접 호출하는 명령어 에이전트 |
| agent | `agents/` (서브디렉토리) | Claude가 작업 중 호출하는 전문 에이전트 |
| docs | `docs/` | 프로젝트 정보 — 에이전트가 탐색·작성 시 참조 |

## 전체 맵

```
agents/
├── commands/
│   └── commit.md                    [command] 커밋 생성
├── orchestrators/
│   ├── doc-orchestrator.md          [agent]   문서 작업 라우터
│   ├── code-orchestrator.md         [agent]   코드 작성 파이프라인 진입점
│   ├── code-final-destination.md    [agent]   코드 품질 검토 오케스트라
│   ├── scene-orchestrator.md        [agent]   씬 작성 파이프라인 진입점
│   └── scene-final-destination.md  [agent]   씬 품질 검토 오케스트라
├── scanners/
│   ├── claude-md-linter.md          [agent]   .md 린트 검사
│   ├── git-change-reader.md         [agent]   git 변경사항 읽기
│   ├── code-reader.md               [agent]   코드 파일 컨텍스트 수집
│   └── scene-reader.md              [agent]   씬·스크립트·리소스 컨텍스트 수집
├── specialists/
│   ├── code-optimizer.md            [agent]   성능 최적화 검토
│   ├── design-pattern-reviewer.md   [agent]   Godot 패턴 검토
│   ├── naming-reviewer.md           [agent]   코드 네이밍 컨벤션 검토
│   ├── node-data-reviewer.md        [agent]   씬 노드·데이터 적합성 검토
│   ├── tree-structure-reviewer.md   [agent]   씬 트리 구조 적합성 검토
│   └── node-naming-reviewer.md      [agent]   씬 노드명 컨벤션 검토
└── writers/
    ├── claude-md-author.md          [agent]   .md 파일 작성
    ├── code-writer.md               [agent]   코드 초안 작성
    ├── code-applier.md              [agent]   코드 파일 적용
    ├── scene-designer.md            [agent]   씬 파일 목록·최상위 구조 설계
    ├── scene-writer.md              [agent]   씬 세부 노드 트리·데이터 값 설계
    └── scene-applier.md             [agent]   노드 트리 기술서 → .tscn 파일 변환

docs/
├── project.md                       [docs]    프로젝트 환경·맥락·규칙
├── structure.md                     [docs]    이 문서 — 에이전트·문서 구조 지도
├── code-writing-pipeline.md         [docs]    코드 작성 파이프라인 전체 흐름
└── scene-writing-pipeline.md        [docs]    씬 작성 파이프라인 전체 흐름

.claude/
├── hooks/
│   ├── commit-guard.sh              커밋 메시지 AI 멘션 차단
│   ├── sensitive-guard.sh           민감 파일 쓰기 차단
│   └── pre-write-claude-md-guard.sh .md 쓰기 시 frontmatter 검증
└── settings.json                    권한·훅 설정
```

## 에이전트 호출 흐름 (문서 관리)

```
사용자
  └─▶ doc-orchestrator
          ├─ [edit]  ──────────────────▶ claude-md-author
          └─ [sync]  ──▶ git-change-reader ──▶ claude-md-author
```

## 에이전트 호출 흐름 (코드 작성)

```
사용자
  └─▶ code-orchestrator
          ├─▶ code-reader
          ├─▶ code-writer (초안)
          ├─▶ code-final-destination
          │       ├─▶ code-optimizer
          │       ├─▶ design-pattern-reviewer
          │       └─▶ naming-reviewer
          │       └──(최대 2회)──▶ code-writer (수정)
          └─▶ code-applier
```

## 에이전트 호출 흐름 (씬 작성)

```
사용자
  └─▶ scene-orchestrator
          ├─▶ scene-reader
          ├─▶ scene-designer (씬 파일·루트 구조 설계)
          ├─▶ scene-writer (세부 노드 트리·데이터 값)
          ├─▶ scene-final-destination
          │       ├─▶ node-data-reviewer
          │       ├─▶ tree-structure-reviewer
          │       └─▶ node-naming-reviewer
          │       └──(최대 2회)──▶ scene-writer (수정)
          └─▶ scene-applier
```

## 새 파일 추가 시 규칙

- command 추가 → `agents/commands/<name>.md`, CLAUDE.md Commands 섹션에 등록
- agent 추가 → `agents/<type>/<name>.md`, CLAUDE.md Agents 섹션에 등록
- docs 추가 → `docs/<name>.md`, CLAUDE.md Docs 섹션에 등록
