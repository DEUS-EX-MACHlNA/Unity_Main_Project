# GameOver 씬 수동 설정 가이드 (2026-02-08)

## 현재 상태

### 완료된 사항
- ✅ GameStateManager의 씬 이름을 "GameOver"로 수정
- ✅ GameOver 씬에 기본 구조 생성:
  - Canvas (UI 레이어)
  - EventSystem
  - GameOverText (TextMeshProUGUI 컴포넌트 추가됨)
  - RestartButton (Button 컴포넌트 추가됨)
  - FadeManager (SceneFadeManager 컴포넌트 추가됨)
  - FadeImage (Image 컴포넌트 추가됨)
  - GameManager (GameOverManager 컴포넌트 추가됨)

### 수동 설정 필요 사항

Unity MCP의 제한으로 인해 다음 항목들을 Unity 에디터에서 수동으로 설정해야 합니다:

## 1. 씬 이름 확인 및 수정

**중요**: 현재 씬 파일 이름이 "GameOver 1.unity"입니다. 다음 중 하나를 선택하세요:

### 옵션 A: 씬 파일 이름 변경 (권장)
1. Project 창에서 `Assets/Scenes/Ryu/GameOver 1.unity` 우클릭
2. Rename → `GameOver.unity`로 변경
3. Build Settings에서 씬 이름이 "GameOver"로 표시되는지 확인

### 옵션 B: GameStateManager 씬 이름 변경
- GameStateManager의 Inspector에서 `Game Over Scene Name`을 "GameOver 1"로 변경

## 2. Canvas 설정

1. Hierarchy에서 `Canvas` 선택
2. Inspector에서 다음 설정:
   - **Canvas 컴포넌트**:
     - Render Mode: Screen Space - Overlay
   - **Canvas Scaler 컴포넌트**:
     - UI Scale Mode: Scale With Screen Size
     - Reference Resolution: X: 1920, Y: 1080
   - **RectTransform**: 기본값 유지 (전체 화면)

## 3. GameOverText 설정

1. Hierarchy에서 `Canvas/GameOverText` 선택
2. Inspector에서 다음 설정:
   - **TextMeshProUGUI 컴포넌트**:
     - Text: "게임 오버\n\n인간성이 0%에 도달했습니다."
     - Font Size: 48
     - Alignment: Center, Middle
     - Color: 빨간색 (R:255, G:0, B:0) 또는 어두운 색상
   - **RectTransform**:
     - Anchor: Center, Center
     - Position: X: 0, Y: 100, Z: 0
     - Width: 800
     - Height: 200
   - **MeshRenderer, MeshFilter, MeshCollider 제거** (UI 요소이므로 불필요)

## 4. RestartButton 설정

1. Hierarchy에서 `Canvas/RestartButton` 선택
2. Inspector에서 다음 설정:
   - **Button 컴포넌트**: 기본값 유지
   - **RectTransform**:
     - Anchor: Center, Bottom
     - Position: X: 0, Y: 100, Z: 0
     - Width: 200
     - Height: 60
   - **MeshRenderer, MeshFilter, MeshCollider 제거** (UI 요소이므로 불필요)
3. **Button의 Text 설정**:
   - RestartButton 하위에 TextMeshPro 자식이 없으면 생성:
     - Hierarchy에서 `RestartButton` 우클릭 → UI → Text - TextMeshPro
   - TextMeshPro 컴포넌트:
     - Text: "재시작"
     - Font Size: 36
     - Alignment: Center, Middle
     - Color: 흰색

## 5. FadeImage 설정

1. Hierarchy에서 `Canvas/FadeImage` 선택
2. Inspector에서 다음 설정:
   - **Image 컴포넌트**:
     - Color: 검은색 (R:0, G:0, B:0, A:255)
     - Raycast Target: 체크 해제 (페이드 중에도 클릭 가능하도록)
   - **RectTransform**:
     - Anchor: Stretch, Stretch
     - Left: 0, Right: 0, Top: 0, Bottom: 0
     - (전체 화면을 덮도록 설정)
   - **MeshRenderer, MeshFilter, MeshCollider 제거** (UI 요소이므로 불필요)

## 6. SceneFadeManager 참조 연결

1. Hierarchy에서 `Canvas/FadeManager` 선택
2. Inspector의 **SceneFadeManager 컴포넌트**:
   - Fade Image: `Canvas/FadeImage` 드래그 앤 드롭
   - Default Fade Duration: 1

## 7. GameOverManager 참조 연결

1. Hierarchy에서 `GameManager` 선택
2. Inspector의 **GameOverManager 컴포넌트**:
   - Game Over Text: `Canvas/GameOverText` 드래그 앤 드롭
   - Restart Button: `Canvas/RestartButton` 드래그 앤 드롭
   - Tutorial Scene Name: "Tutorial" (기본값 확인)
   - Fade Manager: `Canvas/FadeManager`의 SceneFadeManager 컴포넌트 드래그 앤 드롭
   - Fade Duration: 1 (기본값 확인)

## 8. Main Camera 확인

GameOver 씬에 Main Camera가 없으면:
1. Hierarchy에서 우클릭 → Camera
2. 이름을 "Main Camera"로 변경
3. Tag를 "MainCamera"로 설정

## 9. 최종 검증

### 체크리스트
- [ ] Canvas가 Screen Space - Overlay 모드로 설정됨
- [ ] GameOverText가 중앙에 표시되고 텍스트가 올바름
- [ ] RestartButton이 하단 중앙에 배치되고 클릭 가능함
- [ ] FadeImage가 전체 화면을 덮도록 설정됨
- [ ] SceneFadeManager의 Fade Image 참조가 연결됨
- [ ] GameOverManager의 모든 참조가 연결됨
- [ ] Main Camera가 존재함
- [ ] 씬 이름이 "GameOver"로 설정됨 (또는 GameStateManager가 올바른 씬 이름을 참조)

## 테스트 방법

1. **게임 오버 전환 테스트**:
   - 게임 실행
   - HumanityTestHelper를 사용하여 인간성을 0%로 설정
   - GameOver 씬으로 전환되는지 확인
   - 페이드 효과가 적용되는지 확인

2. **재시작 기능 테스트**:
   - GameOver 씬에서 재시작 버튼 클릭
   - Tutorial 씬으로 전환되는지 확인
   - GameStateManager가 재생성되어 초기 상태인지 확인
   - 인간성이 100%로 초기화되었는지 확인
   - 날짜가 1일차로 초기화되었는지 확인

## 문제 해결

### 씬 전환이 안 될 때
- GameStateManager의 Inspector에서 `Game Over Scene Name`이 올바른지 확인
- Build Settings에 GameOver 씬이 포함되어 있는지 확인

### UI가 보이지 않을 때
- Canvas의 Render Mode가 Screen Space - Overlay인지 확인
- Camera가 존재하는지 확인
- UI 요소의 RectTransform이 올바르게 설정되었는지 확인

### 버튼이 작동하지 않을 때
- EventSystem이 씬에 존재하는지 확인
- Button 컴포넌트가 올바르게 연결되었는지 확인
- GameOverManager의 Restart Button 참조가 연결되었는지 확인

