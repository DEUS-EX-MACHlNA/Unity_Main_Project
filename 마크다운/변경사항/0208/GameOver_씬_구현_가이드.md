# Game Over 씬 구현 가이드 (2026-02-08)

## 개요
인간성이 0% 이하가 될 경우 Game Over 씬으로 전환되는 로직을 구현했습니다. Game Over 씬에서는 게임을 재시작할 수 있습니다.

## 구현된 기능

### 1. GameStateManager.cs 수정
- **TriggerGameOver() 메서드 구현**:
  - SceneFadeManager를 사용하여 GameOver 씬으로 전환
  - 페이드 효과 적용 (기존 씬 전환과 동일)
  - Inspector에서 씬 이름과 페이드 시간 설정 가능

### 2. GameOverManager.cs 생성
- **위치**: `Assets/Scripts/Ryu/GameOver/GameOverManager.cs`
- **기능**:
  - Game Over 텍스트 표시
  - 재시작 버튼 기능
  - GameStateManager 재생성 (싱글톤 완전 초기화)
  - Tutorial 씬으로 전환

### 3. GameOver 씬 생성
- **위치**: `Assets/Scenes/Ryu/GameOver.unity`
- **상태**: 씬이 생성되었으나 UI 구성 필요

## Unity 씬 설정 (수동 작업 필요)

### GameOver 씬 UI 구성

1. **Canvas 생성**:
   - Hierarchy에서 우클릭 → UI → Canvas
   - Canvas 설정:
     - Render Mode: Screen Space - Overlay
     - Canvas Scaler: UI Scale Mode = Scale With Screen Size
     - Reference Resolution: 1920 x 1080

2. **EventSystem 생성**:
   - Hierarchy에서 우클릭 → UI → Event System
   - (Canvas 생성 시 자동으로 생성될 수 있음)

3. **Game Over 텍스트 생성**:
   - Canvas 하위에서 우클릭 → UI → Text - TextMeshPro
   - 이름: `GameOverText`
   - 설정:
     - Text: "게임 오버\n\n인간성이 0%에 도달했습니다."
     - Font Size: 48
     - Alignment: Center, Middle
     - Color: 빨간색 또는 어두운 색상
     - RectTransform: 중앙 정렬

4. **재시작 버튼 생성**:
   - Canvas 하위에서 우클릭 → UI → Button - TextMeshPro
   - 이름: `RestartButton`
   - 설정:
     - Button Text: "재시작"
     - Font Size: 36
     - RectTransform: 중앙 하단 배치

5. **SceneFadeManager 설정**:
   - Canvas 하위에 빈 GameObject 생성: `FadeManager`
   - `SceneFadeManager` 컴포넌트 추가
   - Canvas 하위에 Image 생성: `FadeImage`
     - Color: 검은색 (R:0, G:0, B:0, A:255)
     - RectTransform: 전체 화면 덮기 (Anchor: Stretch, Stretch)
     - Raycast Target: 체크 해제
   - FadeManager의 SceneFadeManager 컴포넌트에 FadeImage 연결

6. **GameOverManager 설정**:
   - 빈 GameObject 생성: `GameManager`
   - `GameOverManager` 컴포넌트 추가
   - Inspector 설정:
     - Game Over Text: `GameOverText` 연결
     - Restart Button: `RestartButton` 연결
     - Tutorial Scene Name: "Tutorial"
     - Fade Manager: `FadeManager`의 SceneFadeManager 연결
     - Fade Duration: 1

### Build Settings에 씬 추가

1. File → Build Settings
2. Scenes In Build에 다음 씬들이 포함되어 있는지 확인:
   - Night (Build Index: 1)
   - Tutorial (Build Index: 2)
   - **GameOver (새로 추가 필요)**
3. GameOver 씬 추가:
   - Project 창에서 `Assets/Scenes/Ryu/GameOver.unity` 드래그 앤 드롭
   - 또는 "Add Open Scenes" 버튼 클릭

## 동작 흐름

```
게임 플레이 중
  ↓
인간성 0% 도달
  ↓
GameStateManager.ModifyHumanity() → TriggerGameOver() 호출
  ↓
SceneFadeManager를 찾아서 GameOver 씬으로 전환 (페이드 효과)
  ↓
GameOver 씬 로드
  ↓
GameOverManager.Start() → UI 초기화
  ↓
재시작 버튼 클릭
  ↓
ResetGameState() → GameStateManager 재생성
  ↓
Tutorial 씬으로 전환 (페이드 효과)
  ↓
게임 재시작
```

## 코드 구조

### GameStateManager.cs
```csharp
[Header("Game Over Settings")]
[SerializeField] private string gameOverSceneName = "GameOver";
[SerializeField] private float gameOverFadeDuration = 1f;

private void TriggerGameOver()
{
    SceneFadeManager fadeManager = FindObjectOfType<SceneFadeManager>();
    if (fadeManager != null)
    {
        fadeManager.LoadSceneWithFade(gameOverSceneName, gameOverFadeDuration);
    }
    else
    {
        SceneManager.LoadScene(gameOverSceneName);
    }
}
```

### GameOverManager.cs
```csharp
private void OnRestartButtonClicked()
{
    // GameStateManager 재생성
    ResetGameState();
    
    // Tutorial 씬으로 전환
    fadeManager.LoadSceneWithFade(tutorialSceneName, fadeDuration);
}

private void ResetGameState()
{
    // 기존 인스턴스 제거
    if (GameStateManager.Instance != null)
    {
        Destroy(GameStateManager.Instance.gameObject);
        GameStateManager.Instance = null;
    }
    
    // 새로운 인스턴스 생성
    GameObject gameStateManagerObj = new GameObject("GameStateManager");
    gameStateManagerObj.AddComponent<GameStateManager>();
}
```

## 검증 사항

### 자동 검증
- ✅ GameStateManager.cs 컴파일: 오류 0개, 경고 0개
- ✅ GameOverManager.cs 컴파일: 오류 0개, 경고 0개

### 수동 검증 필요
1. **Game Over 씬 전환**:
   - 인간성을 0%로 만드는 테스트
   - GameOver 씬으로 전환되는지 확인
   - 페이드 효과가 적용되는지 확인

2. **재시작 기능**:
   - 재시작 버튼 클릭
   - Tutorial 씬으로 전환되는지 확인
   - GameStateManager가 재생성되어 초기 상태인지 확인
   - 인간성이 100%로 초기화되었는지 확인
   - 날짜가 1일차로 초기화되었는지 확인

3. **Build Settings**:
   - GameOver 씬이 Build Settings에 포함되어 있는지 확인

## 주의사항

- GameOver 씬의 UI 구성은 Unity 에디터에서 수동으로 설정해야 합니다.
- SceneFadeManager는 GameOver 씬에도 필요합니다 (재시작 시 페이드 효과를 위해).
- GameStateManager 재생성 시 모든 상태가 초기화됩니다 (인간성 100%, 날짜 1일차).

