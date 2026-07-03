# 게임잼 Claude Config

## NEVER

- ❌ `git commit` 직접 실행 금지 — `/commit` 사용.
- ❌ 커밋 메시지에 AI/Claude/Anthropic 언급 금지.
- ❌ `.claude/**/*.md` 내 절대경로(`/Users/...`) 사용 금지.
- ❌ 에이전트·스킬 파일에 regex 패턴 인라인 금지 — 전용 파일에 분리.

## Commands

- `/commit` — conventional commit 형식으로 git 커밋 생성. AI 멘션 가드 포함.

## Agents

**문서 관리**
- `doc-orchestrator` — .claude/ 문서 작업 진입점. edit / sync 분류 후 하위 에이전트 위임.
- `git-change-reader` — 마지막 `docs(sync):` 커밋 이후 변경사항 요약 반환. 읽기 전용.
- `claude-md-author` — .md 파일 작성·재작성. `claude-md-linter` 자체 검증 포함.
- `claude-md-linter` — .md 파일 lint 검사 (길이·어조·구조·frontmatter). 읽기 전용.

**코드 작성**
- `code-orchestrator` — 코드 작성 파이프라인 진입점. 전 단계 순차 조율 후 사용자 보고.
- `code-reader` — 태스크 관련 파일 읽기·컨텍스트 수집. 읽기 전용.
- `code-writer` — 계획서 기반 코드 초안 작성. 수정 요청 수용.
- `code-final-destination` — 3개 전문가 에이전트 병렬 호출, 피드백 종합, 최대 2회 수정 요청.
- `code-optimizer` — 성능·GC·Godot 특화 최적화 검토. 읽기 전용.
- `design-pattern-reviewer` — Godot 아키텍처·신호·씬 패턴 검토. 읽기 전용.
- `naming-reviewer` — C#/GDScript 네이밍 컨벤션 검토. 읽기 전용.
- `code-applier` — code-final-destination 승인 코드를 실제 파일에 적용.

**씬 작성**
- `scene-orchestrator` — 씬 작성 파이프라인 진입점. 전 단계 순차 조율 후 사용자 보고.
- `scene-reader` — 관련 .tscn·.tres·스크립트 읽기·컨텍스트 수집. 읽기 전용.
- `scene-designer` — 씬 파일 목록, 루트 노드 타입, 씬 간 참조 구조 설계.
- `scene-writer` — 세부 노드 트리·인스펙터 값 설계. 수정 요청 수용.
- `scene-final-destination` — 3개 전문가 에이전트 병렬 호출, 피드백 종합, 최대 2회 수정 요청.
- `node-data-reviewer` — 노드 타입·인스펙터 값 적합성 검토. 읽기 전용.
- `tree-structure-reviewer` — 트리 깊이·너비, 서브씬 분리 필요성 검토. 읽기 전용.
- `node-naming-reviewer` — 전체 노드명 컨벤션·명확성 검토. 읽기 전용.
- `scene-applier` — scene-final-destination 승인 기술서를 실제 .tscn 파일로 변환·저장.

## Docs

- [project.md](.claude/docs/project.md) — 엔진 환경, 맥락, 팀 정보, 문서 관리 규칙.
- [structure.md](.claude/docs/structure.md) — .claude/ 전체 구조 지도 (command / agent / docs 분류).
- [folders.md](.claude/docs/folders.md) — **Godot 프로젝트 폴더 구조 및 역할 정의. reader·applier 에이전트 필독.**
- [code-writing-pipeline.md](.claude/docs/code-writing-pipeline.md) — 코드 작성 에이전트 파이프라인 전체 흐름.
- [scene-writing-pipeline.md](.claude/docs/scene-writing-pipeline.md) — 씬 작성 에이전트 파이프라인 전체 흐름.

## Hooks (auto-active)

- **commit-guard** (PreToolUse/Bash) — 커밋 메시지 AI 멘션 차단.
- **sensitive-guard** (PreToolUse/Edit|Write) — .env·크리덴셜 파일 쓰기 차단.
- **pre-write-claude-md-guard** (PreToolUse/Edit|Write) — 에이전트 frontmatter 및 .claude/ 규칙 강제.

## NEVER (bottom anchor)

- ❌ `git commit` 직접 실행 금지 — `/commit` 사용.
- ❌ 문서 담당자 외 .claude/ 파일 수정 금지.
