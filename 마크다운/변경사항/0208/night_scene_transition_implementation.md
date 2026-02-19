# Night Scene 전환 로직 구현 (2026-02-08)

## 개요
Night Scene에서 모든 대화가 종료된 후 다음 날(Tutorial Scene)로 넘어가는 로직을 구현했습니다. 이 과정에서 날짜 관리, 인간성 감소, 씬 전환 페이드 효과, 그리고 턴 리셋 기능이 추가되었습니다.

## 변경된 스크립트

### 1. [GameStateManager.cs]
- **날짜 관리 추가**:
    - `currentDay` 변수 (초기값 1) 및 `MAX_DAY` 상수 (5) 추가.
    - `GetCurrentDay()` 메서드 추가.
- **다음 날 진행 로직 (`AdvanceToNextDay`)**:
    - 날짜를 1 증가시킵니다.
    - **인간성 감소**: 하루가 지날 때마다 인간성이 10% 감소하도록 `ModifyHumanity(-10f)`를 호출합니다.
    - `OnDayChanged` 이벤트를 발생시켜 다른 시스템에 알립니다.

### 2. [NightDialogueManager.cs]
- **씬 전환 통합**:
    - `SceneFadeManager` 참조 변수 추가.
    - `OnAllDialoguesComplete()` 메서드에서 대화 종료 시 `GameStateManager.Instance.AdvanceToNextDay()`를 호출하고, `SceneFadeManager`를 통해 Tutorial 씬으로 전환하도록 구현했습니다.

### 3. [TurnManager.cs]
- **턴 리셋 자동화**:
    - `GameStateManager.OnDayChanged` 이벤트를 구독하여, 날짜가 변경될 때마다 `ResetTurns()`를 호출해 턴 수를 초기화합니다.

## Unity 씬 설정 (Night Scene)
Unity MCP를 통해 다음 설정이 자동으로 적용되었습니다:
1. **FadeImage 생성**: Canvas 하위에 투명한 검은색 이미지를 가진 `FadeImage` GameObject를 생성하고 `SceneFadeManager` 컴포넌트를 추가했습니다.
2. **참조 연결**: `GameManager`의 `NightDialogueManager` 컴포넌트에 생성된 `SceneFadeManager`를 연결하고, 이동할 씬 이름을 "Tutorial"로 설정했습니다.
