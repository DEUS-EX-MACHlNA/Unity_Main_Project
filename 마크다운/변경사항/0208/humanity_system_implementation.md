# 인간성 시스템 구현 내역 (2026-02-08)

## 1. 개요
백엔드 API와 연동하여 플레이어의 인간성(Humanity) 수치를 관리하고, 이를 UI에 표시하는 시스템을 구현했습니다.

## 2. 구현 상세

### 2.1 GameStateManager (상태 관리)
- **파일**: `Assets/Scripts/Ryu/Global/GameStateManager.cs`
- **역할**: 게임 전반의 상태를 중앙에서 관리하는 싱글톤 매니저
- **주요 기능**:
  - `humanity` 변수 (0~100, 초기값 100) 관리
  - `OnHumanityChanged` 이벤트 발행: 수치 변경 시 UI 등에 알림
  - `OnHumanityReachedZero` 이벤트 발행: 0% 도달 시 게임 오버 처리 (트리거 구현 완료)
- **씬 설정**: `GameStateManager` 오브젝트 생성 및 컴포넌트 추가 완료

### 2.2 ApiClient (통신 확장)
- **파일**: `Assets/Scripts/Ryu/Global/ApiClient.cs`
- **변경 사항**:
  - `GameResponse` 클래스에 `humanity_change` 필드 추가
  - `SendMessage` 메서드 콜백 시그니처 변경 (`Action<string>` -> `Action<string, float>`)
  - 응답 파싱 시 인간성 변화량을 추출하여 전달하도록 수정

### 2.3 InputHandler (연동)
- **파일**: `Assets/Scripts/Ryu/Tutorial/InputHandler.cs`
- **변경 사항**:
  - `GameStateManager` 참조 추가
  - API 성공 콜백(`OnApiSuccess`)에서 인간성 변화량을 `GameStateManager`에 전달
  - `GameManager` 오브젝트에 `GameStateManager` 참조 연결 완료

### 2.4 HumanityText (UI)
- **파일**: `Assets/Scripts/Ryu/UI/HumanityText.cs`
- **기능**:
  - `GameStateManager`의 `OnHumanityChanged` 이벤트를 구독하여 수치 업데이트
  - 수치에 따른 텍스트 색상 변경 (초록 -> 노란색 -> 빨간색)
- **씬 설정**:
  - `HUD_TopLeft` 하위에 `HumanityText` 오브젝트 생성 및 배치
  - `TextMeshProUGUI` 연결 및 앵커/위치 정렬 완료

## 3. 검증 방법
1. **Play Mode** 실행
2. 좌측 상단 HUD에 "인간성: 100.0%" (초록색) 표시 확인
3. AI와의 대화 진행 시 인간성 변화량에 따라 수치와 색상이 변경되는지 확인
