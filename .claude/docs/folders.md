# 프로젝트 폴더 구조

Godot 프로젝트 루트: `test/`

reader·applier 에이전트는 이 문서를 기준으로 파일 경로를 결정한다.

## 폴더 목록

| 폴더 | 경로 | 역할 |
|---|---|---|
| autoload | `test/autoload/` | 오토로드 관련 씬 및 스크립트 |
| class | `test/class/` | 나머지 class 유형에 해당하지 않는 스크립트 |
| class_core | `test/class_core/` | 여러 인스턴스를 생성하지 않는, 인게임 내 Manager 역할을 하는 핵심 고유 class |
| class_resource | `test/class_resource/` | Resource 또는 RefCounted를 상속하는 class |
| class_util | `test/class_util/` | 유틸리티 class |
| class_visual | `test/class_visual/` | 비주얼 출력 관련 class |
| resource_dev | `test/resource_dev/` | Panel, Font, Shader 등 씬 내 사용하는 Godot 기본 Resource |
| resource_ingame | `test/resource_ingame/` | class_resource로 정의한 커스텀 Resource 인스턴스 |
| scene_major | `test/scene_major/` | 월드나 Manager 역할을 하는 고유 씬 (단일 인스턴스) |
| scene_minor | `test/scene_minor/` | 버튼, Entity, 오브젝트 등 여럿 instantiate 가능한 씬 |

## 파일 배치 원칙

- 새 스크립트·씬을 만들 때는 이 문서의 역할 정의를 먼저 확인한다.
- 역할이 명확하지 않으면 작업 전에 사용자에게 배치 폴더를 확인한다.

## 프로젝트 루트 파일

| 파일 | 설명 |
|---|---|
| `test/project.godot` | Godot 프로젝트 설정 |
| `test/icon.svg` | 기본 아이콘 |
