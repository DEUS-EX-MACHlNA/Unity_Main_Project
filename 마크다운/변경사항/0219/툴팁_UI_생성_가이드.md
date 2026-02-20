# 툴팁 UI 생성 가이드

## 개요
문 호버 툴팁 시스템을 사용하기 위해 Unity 에디터에서 UI 구조를 생성해야 합니다.

## 생성 단계

### Step 1: GameCanvas 확인
1. Unity 에디터에서 현재 씬을 엽니다 (예: PlayersRoom)
2. Hierarchy에서 `GameCanvas` 오브젝트를 찾습니다
3. GameCanvas가 없으면 생성: `GameObject > UI > Canvas`

### Step 2: TooltipPanel 생성
1. Hierarchy에서 `GameCanvas` 선택
2. 우클릭 → `UI > Panel` 선택
3. 생성된 Panel의 이름을 `TooltipPanel`로 변경

### Step 3: TooltipPanel 설정
1. `TooltipPanel` 선택
2. Inspector에서 다음 설정:
   - **RectTransform**:
     - Anchor Presets: `Bottom-Left` (Alt+Shift 클릭)
     - Pos X: 0
     - Pos Y: 0
     - Width: 250 (임시, 자동 조정됨)
     - Height: 100 (임시, 자동 조정됨)
     - **참고**: 초기 위치는 중요하지 않습니다. 툴팁이 표시될 때 마우스 위치에 따라 자동으로 이동합니다.
   - **Image 컴포넌트**:
     - Color: R=0, G=0, B=0, A=204 (검은색 반투명, alpha 0.8)
   - **CanvasGroup 컴포넌트 추가**:
     - `Add Component > UI > Canvas Group`
   - **초기 상태**: `SetActive(false)` 체크 해제 (비활성화)

### Step 4: TitleText 생성
1. `TooltipPanel` 선택
2. 우클릭 → `UI > Text - TextMeshPro` 선택
3. 이름을 `TitleText`로 변경
4. Inspector 설정:
   - **RectTransform**:
     - Anchor Presets: `Stretch-Stretch` (Alt+Shift 클릭)
     - Left: 10
     - Right: -10
     - Top: -10
     - Bottom: 10
   - **TextMeshProUGUI**:
     - Text: "거실로 이동" (임시)
     - Font Size: 18
     - Alignment: Center (가로), Middle (세로)
     - Color: White (R=255, G=255, B=255, A=255)
     - Auto Size: 체크 해제

### Step 5: TooltipPanel 레이아웃 설정 (자동 크기 조정)
1. `TooltipPanel` 선택
2. **Content Size Fitter 컴포넌트 추가**:
   - `Add Component > Layout > Content Size Fitter`
   - Horizontal Fit: `Preferred Size`
   - Vertical Fit: `Preferred Size`
3. **Layout Group 설정** (선택적, 더 나은 레이아웃을 위해):
   - `Add Component > Layout > Vertical Layout Group`
   - Padding: Left=10, Right=10, Top=10, Bottom=10
   - Spacing: 5
   - Child Alignment: Middle Center
   - Child Force Expand: Width 체크, Height 체크 해제

### Step 6: DoorTooltipManager 오브젝트 생성
1. `GameCanvas` 선택
2. 우클릭 → `Create Empty` 선택
3. 이름을 `DoorTooltipManager`로 변경
4. **DoorTooltipManager 컴포넌트 추가**:
   - `Add Component > Scripts > DoorTooltipManager`
5. Inspector에서 설정:
   - **Tooltip Panel**: `TooltipPanel` 드래그 앤 드롭
   - **Title Text**: `TitleText` 드래그 앤 드롭
   - **Background Image**: `TooltipPanel`의 Image 컴포넌트 드래그 앤 드롭
   - **Show Delay**: 0.3
   - **Cursor Offset**: X=20, Y=20
   - **Background Color**: R=0, G=0, B=0, A=204 (검은색 반투명)

## 최종 구조
```
GameCanvas
├── DoorTooltipManager (GameObject with DoorTooltipManager component)
└── TooltipPanel (Panel, initially inactive)
    └── TitleText (TextMeshProUGUI)
```

## 주의사항
- TooltipPanel은 초기에 비활성화되어 있어야 합니다
- TextMeshPro를 사용하므로 TextMeshPro 패키지가 설치되어 있어야 합니다
- 모든 씬에 동일한 구조를 생성할 필요는 없습니다 (DontDestroyOnLoad 사용)
- 첫 번째 씬에만 DoorTooltipManager를 배치하면 다른 씬에서도 사용 가능합니다

## 테스트
1. Play 모드 진입
2. "toDoor" 오브젝트에 마우스를 올림
3. 0.3초 후 툴팁이 표시되는지 확인
4. 마우스를 벗어나면 툴팁이 사라지는지 확인

