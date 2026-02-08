# Game Over 씬 전환 로직 구현 (2026-02-08)

## 개요
인간성이 0% 이하가 될 경우 Game Over 씬으로 전환되는 로직을 구현했습니다. Game Over 씬에서는 게임을 재시작할 수 있으며, 재시작 시 GameStateManager가 완전히 재생성되어 모든 상태가 초기화됩니다.

## 구현된 기능

### 1. GameStateManager.cs - Game Over 기능 추가

**파일**: [Assets/Scripts/Ryu/Global/GameStateManager.cs](Assets/Scripts/Ryu/Global/GameStateManager.cs)

#### 변경 사항
- **`TriggerGameOver()` 메서드 구현**:
  - SceneFadeManager를 사용하여 GameOver 씬으로 전환
  - 페이드 효과 적용 (기존 씬 전환과 동일)
  - SceneFadeManager를 찾을 수 없으면 페이드 없이 전환

- **Inspector 설정 필드 추가**:
  - `gameOverSceneName` (기본값: "GameOver")
  - `gameOverFadeDuration` (기본값: 1초)

#### 코드 구조
```164:186:Assets/Scripts/Ryu/Global/GameStateManager.cs
[Header("Game Over Settings")]
[SerializeField] private string gameOverSceneName = "GameOver";
[SerializeField] private float gameOverFadeDuration = 1f;

/// <summary>
/// 인간성 0% 도달 시 배드 엔딩을 트리거합니다.
/// </summary>
private void TriggerGameOver()
{
    Debug.LogWarning("[GameStateManager] 인간성이 0%에 도달했습니다. 게임 오버!");
    
    // SceneFadeManager를 찾아서 GameOver 씬으로 전환
    SceneFadeManager fadeManager = FindObjectOfType<SceneFadeManager>();
    if (fadeManager != null)
    {
        fadeManager.LoadSceneWithFade(gameOverSceneName, gameOverFadeDuration);
    }
    else
    {
        Debug.LogWarning("[GameStateManager] SceneFadeManager를 찾을 수 없습니다. 페이드 없이 씬을 전환합니다.");
        SceneManager.LoadScene(gameOverSceneName);
    }
}
```

#### 트리거 조건
- `ModifyHumanity()` 메서드에서 인간성이 0% 이하로 떨어질 때 자동 호출
- `OnHumanityReachedZero` 이벤트 발생 후 `TriggerGameOver()` 호출

### 2. GameOverManager.cs 생성

**파일**: [Assets/Scripts/Ryu/GameOver/GameOverManager.cs](Assets/Scripts/Ryu/GameOver/GameOverManager.cs)

#### 주요 기능
- **Game Over 텍스트 표시**: "게임 오버\n\n인간성이 0%에 도달했습니다."
- **재시작 버튼 기능**: Tutorial 씬으로 전환하여 게임 재시작
- **GameStateManager 재생성**: 싱글톤 완전 초기화
- **자동 참조 찾기**: Awake()에서 GameOverText, RestartButton, SceneFadeManager 자동 찾기

#### 코드 구조
```20:45:Assets/Scripts/Ryu/GameOver/GameOverManager.cs
private void Awake()
{
    // Inspector에서 설정되지 않은 경우 자동으로 찾기
    if (gameOverText == null)
    {
        GameObject textObj = GameObject.Find("GameOverText");
        if (textObj != null)
        {
            gameOverText = textObj.GetComponent<TextMeshProUGUI>();
        }
    }

    if (restartButton == null)
    {
        GameObject buttonObj = GameObject.Find("RestartButton");
        if (buttonObj != null)
        {
            restartButton = buttonObj.GetComponent<Button>();
        }
    }

    if (fadeManager == null)
    {
        fadeManager = FindObjectOfType<SceneFadeManager>();
    }
}
```

```77:116:Assets/Scripts/Ryu/GameOver/GameOverManager.cs
private void OnRestartButtonClicked()
{
    Debug.Log("[GameOverManager] 게임 재시작");

    // GameStateManager 재생성 (싱글톤 재초기화)
    ResetGameState();

    // Tutorial 씬으로 전환
    if (fadeManager != null)
    {
        fadeManager.LoadSceneWithFade(tutorialSceneName, fadeDuration);
    }
    else
    {
        Debug.LogWarning("[GameOverManager] SceneFadeManager가 연결되지 않았습니다. 페이드 없이 씬을 전환합니다.");
        SceneManager.LoadScene(tutorialSceneName);
    }
}

/// <summary>
/// 게임 상태를 완전히 초기화합니다. GameStateManager 싱글톤을 재생성합니다.
/// </summary>
private void ResetGameState()
{
    // 기존 GameStateManager 인스턴스 제거
    if (GameStateManager.Instance != null)
    {
        GameObject oldInstance = GameStateManager.Instance.gameObject;
        Destroy(oldInstance);
        // Instance는 private setter이므로 직접 null 설정 불가
        // Destroy 후 새 인스턴스 생성 시 Awake()에서 자동으로 Instance가 설정됨
    }

    // 새로운 GameStateManager 생성
    // Awake()에서 자동으로 Instance가 설정됨
    GameObject gameStateManagerObj = new GameObject("GameStateManager");
    GameStateManager newManager = gameStateManagerObj.AddComponent<GameStateManager>();
    
    Debug.Log("[GameOverManager] GameStateManager 재생성 완료");
}
```

### 3. SceneFadeManager.cs 개선

**파일**: [Assets/Scripts/Ryu/Tutorial/SceneFadeManager.cs](Assets/Scripts/Ryu/Tutorial/SceneFadeManager.cs)

#### 변경 사항
- **`Awake()`에서 FadeImage 자동 찾기 기능 추가**:
  - Inspector에서 연결하지 않아도 자동으로 "FadeImage" GameObject를 찾아서 연결
  - 기존 씬과의 호환성 유지

#### 코드 구조
```16:23:Assets/Scripts/Ryu/Tutorial/SceneFadeManager.cs
private void Awake()
{
    // Inspector에서 설정되지 않은 경우 자동으로 찾기
    if (fadeImage == null)
    {
        GameObject imageObj = GameObject.Find("FadeImage");
        if (imageObj != null)
        {
            fadeImage = imageObj.GetComponent<Image>();
        }
    }

    if (fadeImage != null)
    {
        // 초기 상태: 투명
        fadeImage.color = new Color(0, 0, 0, 0);
    }
}
```

### 4. GameOver 씬 생성 및 구성

**파일**: `Assets/Scenes/Ryu/GameOver.unity`

#### 생성된 오브젝트 및 설정

1. **Canvas**
   - 컴포넌트: Canvas, CanvasScaler, GraphicRaycaster
   - Render Mode: Screen Space - Overlay (기본값)
   - 불필요한 컴포넌트 제거됨 (MeshRenderer, MeshFilter, MeshCollider)

2. **EventSystem**
   - 컴포넌트: EventSystem, StandaloneInputModule
   - UI 상호작용을 위한 필수 시스템
   - 불필요한 컴포넌트 제거됨

3. **GameOverText** (Canvas 하위)
   - 컴포넌트: RectTransform, CanvasRenderer, TextMeshProUGUI
   - 텍스트: "게임 오버\n\n인간성이 0%에 도달했습니다."
   - 폰트 크기: 48
   - 정렬: Center, Middle
   - 위치: (0, 100, 0)
   - 크기: 800x200
   - 불필요한 컴포넌트 제거됨

4. **RestartButton** (Canvas 하위)
   - 컴포넌트: RectTransform, Button
   - 위치: (0, -200, 0)
   - 크기: 200x60
   - 하위 TextMeshPro 텍스트: "재시작" (폰트 크기 36)
   - 불필요한 컴포넌트 제거됨

5. **FadeManager** (Canvas 하위)
   - 컴포넌트: Transform, SceneFadeManager
   - SceneFadeManager 컴포넌트가 자동으로 FadeImage를 찾음
   - 불필요한 컴포넌트 제거됨

6. **FadeImage** (Canvas 하위)
   - 컴포넌트: RectTransform, CanvasRenderer, Image
   - Color: 검은색 (자동 설정)
   - Raycast Target: false (페이드 중에도 클릭 가능)
   - 불필요한 컴포넌트 제거됨

7. **GameManager**
   - 컴포넌트: Transform, GameOverManager
   - GameOverManager가 자동으로 필요한 참조를 찾음
   - 불필요한 컴포넌트 제거됨

8. **Camera**
   - 컴포넌트: Camera, AudioListener
   - UI 렌더링을 위한 필수 카메라

## 동작 흐름

```
게임 플레이 중
  ↓
인간성 수치 변경 (ModifyHumanity 호출)
  ↓
인간성 0% 이하 도달
  ↓
OnHumanityReachedZero 이벤트 발생
  ↓
TriggerGameOver() 호출
  ↓
SceneFadeManager 찾기 (FindObjectOfType)
  ↓
GameOver 씬으로 전환 (페이드 아웃 → 씬 로드 → 페이드 인)
  ↓
GameOver 씬 로드
  ↓
GameOverManager.Awake() → 자동 참조 찾기
  ↓
GameOverManager.Start() → UI 초기화, 버튼 이벤트 연결
  ↓
재시작 버튼 클릭
  ↓
OnRestartButtonClicked() 호출
  ↓
ResetGameState() → GameStateManager 재생성
  ↓
Tutorial 씬으로 전환 (페이드 효과)
  ↓
게임 재시작 (인간성 100%, 날짜 1일차, 턴수 10)
```

## 주요 기능 상세

### 1. 게임 오버 감지 및 전환
- **감지 시점**: `ModifyHumanity()` 메서드에서 인간성이 0% 이하로 떨어질 때
- **전환 방식**: SceneFadeManager를 사용한 페이드 전환
- **대체 방식**: SceneFadeManager를 찾을 수 없으면 페이드 없이 즉시 전환

### 2. 자동 참조 찾기
- **GameOverManager**: 
  - GameOverText 자동 찾기 (GameObject.Find)
  - RestartButton 자동 찾기 (GameObject.Find)
  - SceneFadeManager 자동 찾기 (FindObjectOfType)
- **SceneFadeManager**: 
  - FadeImage 자동 찾기 (GameObject.Find)
- **장점**: Inspector에서 수동으로 연결하지 않아도 자동으로 작동

### 3. 게임 재시작
- **GameStateManager 재생성**:
  - 기존 인스턴스 완전 제거 (Destroy)
  - 새로운 GameObject 생성 및 GameStateManager 컴포넌트 추가
  - Awake()에서 자동으로 Instance 설정
- **상태 초기화**:
  - 인간성: 100%
  - 날짜: 1일차
  - 턴수: 10 (Tutorial 씬 로드 시 TurnManager에서 초기화)

### 4. 씬 전환
- **페이드 효과**: 모든 씬 전환에 SceneFadeManager 사용
- **일관성**: 기존 씬 전환(Night, Tutorial)과 동일한 방식

## 파일 변경 내역

### 수정된 파일
1. **`Assets/Scripts/Ryu/Global/GameStateManager.cs`**
   - `TriggerGameOver()` 메서드 구현
   - Game Over 설정 필드 추가

2. **`Assets/Scripts/Ryu/Tutorial/SceneFadeManager.cs`**
   - `Awake()`에서 FadeImage 자동 찾기 기능 추가

### 새로 생성된 파일
1. **`Assets/Scripts/Ryu/GameOver/GameOverManager.cs`**
   - Game Over 씬 관리 스크립트
   - 재시작 기능 구현
   - 자동 참조 찾기 기능

2. **`Assets/Scenes/Ryu/GameOver.unity`**
   - Game Over 씬
   - 모든 UI 요소 구성 완료

## 검증 결과

### 자동 검증
- ✅ GameStateManager.cs 컴파일: 오류 0개, 경고 0개
- ✅ GameOverManager.cs 컴파일: 오류 0개, 경고 0개
- ✅ SceneFadeManager.cs 컴파일: 오류 0개, 경고 0개
- ✅ Unity 컴파일: 성공
- ✅ 린터 검증: 오류 없음

### Unity MCP 검증
- ✅ 스크립트 검증: 모든 스크립트 정상
- ✅ 씬 저장: GameOver 씬 저장 완료
- ✅ 오브젝트 생성: 모든 필수 오브젝트 생성 확인
- ✅ 불필요한 컴포넌트 제거: 완료

### Build Settings
- ✅ GameOver 씬이 Build Settings에 포함됨 (Build Index: 3)

## 수동 검증 필요 사항

1. **게임 오버 전환 테스트**:
   - HumanityTestHelper를 사용하여 인간성을 0%로 설정
   - GameOver 씬으로 전환되는지 확인
   - 페이드 효과가 적용되는지 확인
   - GameOverText가 올바르게 표시되는지 확인

2. **재시작 기능 테스트**:
   - 재시작 버튼 클릭
   - Tutorial 씬으로 전환되는지 확인
   - GameStateManager가 재생성되어 초기 상태인지 확인
   - 인간성이 100%로 초기화되었는지 확인
   - 날짜가 1일차로 초기화되었는지 확인
   - 턴수가 10으로 초기화되었는지 확인

3. **UI 레이아웃 확인**:
   - GameOverText가 화면 중앙에 표시되는지 확인
   - RestartButton이 하단 중앙에 배치되어 있는지 확인
   - 버튼 텍스트가 올바르게 표시되는지 확인

## 구현 특징

### 자동화
- **자동 참조 찾기**: Inspector에서 수동 연결 불필요
- **자동 초기화**: 씬 로드 시 자동으로 UI 초기화
- **자동 이벤트 연결**: 재시작 버튼 클릭 이벤트 자동 연결

### 안정성
- **null 체크**: 모든 참조에 대해 null 체크 수행
- **대체 동작**: SceneFadeManager를 찾을 수 없으면 페이드 없이 전환
- **디버그 로그**: 모든 주요 동작에 디버그 로그 출력

### 확장성
- **Inspector 설정**: 씬 이름, 페이드 시간 등 Inspector에서 변경 가능
- **이벤트 시스템**: OnHumanityReachedZero 이벤트로 다른 시스템과 연동 가능

## 주의사항

- GameStateManager 재생성 시 모든 상태가 초기화됩니다 (인간성 100%, 날짜 1일차)
- SceneFadeManager는 GameOver 씬에도 필요합니다 (재시작 시 페이드 효과를 위해)
- 자동 참조 찾기는 GameObject.Find()를 사용하므로 오브젝트 이름이 정확해야 합니다
- FadeImage의 RectTransform은 Unity 에디터에서 전체 화면을 덮도록 설정하는 것을 권장합니다

## 완료일
2026-02-08

