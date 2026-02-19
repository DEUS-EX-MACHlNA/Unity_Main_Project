# Night 씬 대화 시스템 레이아웃 설정

## 작업일: 2026-02-08

---

## 1. 개요

Night 씬에서 가족들의 대화를 표시하는 대화 시스템의 UI 레이아웃과 폰트 설정을 구현했습니다. 초기에는 코드에서 모든 레이아웃과 폰트를 설정했으나, 이후 Unity 에디터에서 관리하도록 간소화했습니다.

---

## 2. 생성된 파일

### `Assets/Scripts/Ryu/Night/NightDialogueManager.cs` (신규)

**역할**: Night 씬에서 가족들의 대화를 엿듣는 대화 시스템 관리

**주요 기능**:
- 대화 텍스트의 누적 표시 (이전 대화 유지)
- 타이핑 효과로 대사 표시
- 마우스 클릭으로 다음 대사 진행
- 클릭 힌트 표시/숨김

**Inspector 설정 항목**:
| 항목 | 타입 | 기본값 | 설명 |
|------|------|--------|------|
| speakerNameText | TextMeshProUGUI | (비어있음) | 화자 이름 텍스트 (현재 미사용) |
| dialogueText | TextMeshProUGUI | (비어있음) | 대사 내용 텍스트 |
| dialoguePanel | GameObject | (비어있음) | 대화 패널 |
| clickHint | GameObject | (비어있음) | 클릭 힌트 텍스트 |
| typingSpeed | float | 0.05 | 타이핑 효과 속도 (초) |

**주요 메서드**:
```csharp
private void SetupUILayout()              // UI 초기 설정
private void InitializeDialogues()        // 대화 데이터 초기화
private void ShowDialogue(int index)      // 대사 표시
private IEnumerator TypeDialogueAccumulated(string newDialogue)  // 타이핑 효과
private void OnDialogueClick()            // 클릭 이벤트 처리
```

---

## 3. 씬 변경사항 (Night.unity)

### 3.1 UI GameObject 구조

```
Canvas
└── DialoguePanel (Image)
    ├── SpeakerNameText (TextMeshProUGUI) - 비활성화됨
    ├── DialogueText (TextMeshProUGUI)    - 대사 표시
    └── ClickHint (TextMeshProUGUI)        - 클릭 힌트
```

### 3.2 DialoguePanel 설정

**RectTransform**:
- Anchor Preset: **Stretch-Stretch**
- Anchor Min: (0, 0)
- Anchor Max: (1, 1)
- Pivot: (0.5, 0.5)
- Anchored Position: (0, 0)
- Size Delta: (-1200, 0) - 좌우 각각 600씩 여백

**Image 컴포넌트**:
- Color: (R: 0, G: 0, B: 0, A: 0.8) - 반투명 검은색 배경

### 3.3 SpeakerNameText 설정

**RectTransform**:
- Anchor Preset: **Stretch-Top**
- Anchor Min: (0, 1)
- Anchor Max: (1, 1)
- Pivot: (0, 1)
- Anchored Position: (20, -20)
- Size Delta: (-40, 40)

**TextMeshProUGUI**:
- Font Size: 24
- Alignment: Left
- Color: White (1, 1, 1, 1)
- **상태**: 비활성화 (누적 방식에서는 사용하지 않음)

### 3.4 DialogueText 설정

**RectTransform**:
- Anchor Preset: **Stretch-Stretch**
- Anchor Min: (0, 0)
- Anchor Max: (1, 1)
- Pivot: (0.5, 0.5)
- Anchored Position: (0, 0)
- Size Delta: (-40, -120)
- Offset Min: (20, 70) - 상단 여백 (화자 이름 공간)
- Offset Max: (-20, -50) - 하단 여백 (클릭 힌트 공간)

**TextMeshProUGUI**:
- Font Size: **32**
- Alignment: Top Left
- Color: White (1, 1, 1, 1)
- Text Wrapping Mode: Normal (자동 줄바꿈)
- Overflow Mode: Truncate (스크롤 대신 잘림 처리)

### 3.5 ClickHint 설정

**RectTransform**:
- Anchor Preset: **Bottom-Right**
- Anchor Min: (1, 0)
- Anchor Max: (1, 0)
- Pivot: (1, 0)
- Anchored Position: (-20, 20)
- Size Delta: (200, 30)

**TextMeshProUGUI**:
- Font Size: 16
- Alignment: Right
- Color: (R: 0.7, G: 0.7, B: 0.7, A: 1) - 회색
- Text: "클릭하여 계속..."

### 3.6 NightDialogueManager 컴포넌트 (GameManager)

**설정된 변수값**:
- typingSpeed: 0.05
- useScreenBasedSize: true
- screenWidthRatio: 0.7
- screenHeightRatio: 0.4

---

## 4. 레이아웃 설정 상세

### 4.1 DialoguePanel 레이아웃

**설정 방식**: Stretch-Stretch (화면 전체를 채우되 좌우 여백)

```
┌─────────────────────────────────────────┐
│  [600px 여백]  DialoguePanel  [600px 여백]  │
│                                         │
│         (대화 내용 표시 영역)              │
│                                         │
└─────────────────────────────────────────┘
```

- 좌우 각각 600픽셀 여백으로 화면 중앙에 배치
- 세로는 화면 전체를 채움
- 반투명 검은색 배경 (alpha: 0.8)

### 4.2 DialogueText 레이아웃

**설정 방식**: Stretch-Stretch (DialoguePanel 내부 전체 영역)

- 상단 여백: 70픽셀 (화자 이름 공간 확보)
- 하단 여백: 50픽셀 (클릭 힌트 공간 확보)
- 좌우 여백: 각각 20픽셀

### 4.3 ClickHint 레이아웃

**설정 방식**: Bottom-Right (DialoguePanel 우측 하단)

- DialoguePanel의 우측 하단에 고정
- 크기: 200 x 30 픽셀
- 오른쪽 정렬

---

## 5. 코드에서 Unity 씬 파일로 설정 이전

### 5.1 초기 구현 (코드에서 모든 설정)

처음에는 `SetupUILayout()` 메서드에서 모든 레이아웃과 폰트를 코드로 설정했습니다:

```csharp
private void SetupUILayout()
{
    // DialoguePanel RectTransform 설정
    panelRect.anchorMin = new Vector2(0, 0);
    panelRect.anchorMax = new Vector2(1, 1);
    panelRect.offsetMin = new Vector2(600, 0);
    panelRect.offsetMax = new Vector2(-600, 0);
    
    // DialogueText 폰트 크기 설정
    dialogueText.fontSize = 32;
    dialogueText.alignment = TextAlignmentOptions.TopLeft;
    // ... 기타 설정
}
```

**문제점**:
- 코드와 Unity 에디터 설정이 중복됨
- 에디터에서 확인하기 어려움
- 런타임에 코드가 에디터 설정을 덮어씀

### 5.2 Unity 씬 파일에 동기화

코드에 설정된 모든 값들을 Unity 씬 파일에도 동일하게 적용했습니다:

- NightDialogueManager 컴포넌트 변수값 동기화
- DialoguePanel RectTransform 설정
- DialogueText RectTransform 및 TextMeshProUGUI 설정
- ClickHint RectTransform 설정
- SpeakerNameText RectTransform 설정

### 5.3 코드 간소화

Unity 씬 파일에 모든 설정이 적용된 후, 코드에서 레이아웃/폰트 설정 부분을 제거하고 필수 로직만 유지했습니다:

**제거된 부분**:
- 모든 RectTransform 설정 코드
- TextMeshProUGUI의 정적 속성 설정 (fontSize, alignment, color)
- Image 컴포넌트의 color 설정
- 사용하지 않는 변수들 (panelWidth, panelHeight, padding, useScreenBasedSize, screenWidthRatio, screenHeightRatio)

**유지된 부분**:
- `speakerNameText.gameObject.SetActive(false)` - 로직상 필요
- `clickHint.text = "클릭하여 계속..."` - 동적 텍스트 설정

**간소화 결과**:
- 코드 라인 수: 348줄 → 252줄 (약 28% 감소)
- 레이아웃과 폰트는 Unity 에디터에서만 관리
- 코드는 핵심 로직에 집중

---

## 6. 동작 흐름

```
[게임 시작]
  ↓
NightDialogueManager.Start()
  ↓
InitializeDialogues() - 대화 데이터 초기화
  ↓
SetupUILayout() - UI 초기 설정 (필수 로직만)
  ↓
ShowDialogue(0) - 첫 번째 대사 표시
  ↓
TypeDialogueAccumulated() - 타이핑 효과
  ↓
[사용자 클릭]
  ↓
OnDialogueClick()
  ↓
ShowDialogue(다음 인덱스) - 다음 대사 표시
  ↓
[모든 대사 완료]
  ↓
OnAllDialoguesComplete()
```

---

## 7. 대화 표시 방식

### 7.1 누적 방식

기존 대화는 유지하고 새 대사만 추가하는 방식:

```
[첫 번째 대사]
엘리노어 (새엄마): 오늘 우리 아이가...

[두 번째 대사 추가]
엘리노어 (새엄마): 오늘 우리 아이가...
루카스 (동생): 응, 응! 오늘 누나(형)가...

[세 번째 대사 추가]
엘리노어 (새엄마): 오늘 우리 아이가...
루카스 (동생): 응, 응! 오늘 누나(형)가...
아더 (새아빠): 음, 확실히 소란을...
```

### 7.2 타이핑 효과

- 각 문자를 `typingSpeed` (0.05초) 간격으로 표시
- 타이핑 중 클릭 시 즉시 완성
- 타이핑 완료 후 클릭 힌트 표시

---

## 8. 디버그 로그

| 태그 | 내용 |
|------|------|
| `[NightDialogueManager]` | `대사 {index}/{total} 추가: {speakerName}` |
| `[NightDialogueManager]` | `모든 대사 표시 완료` |

---

## 9. 파일 구조

```
Assets/
├── Scenes/
│   └── Ryu/
│       └── Night.unity          ← UI 레이아웃 설정 포함
└── Scripts/
    └── Ryu/
        └── Night/
            └── NightDialogueManager.cs    ← 대화 시스템 로직
```

---

## 10. 레이아웃 설정 요약

### 10.1 DialoguePanel

| 속성 | 값 |
|------|-----|
| Anchor | Stretch-Stretch |
| 좌우 여백 | 각각 600픽셀 |
| 배경 색상 | 검은색 (alpha: 0.8) |

### 10.2 DialogueText

| 속성 | 값 |
|------|-----|
| Anchor | Stretch-Stretch |
| Font Size | 32 |
| Alignment | Top Left |
| 상단 여백 | 70픽셀 |
| 하단 여백 | 50픽셀 |
| 좌우 여백 | 각각 20픽셀 |

### 10.3 ClickHint

| 속성 | 값 |
|------|-----|
| Anchor | Bottom-Right |
| Font Size | 16 |
| Alignment | Right |
| Color | 회색 (0.7, 0.7, 0.7, 1) |
| 크기 | 200 x 30 픽셀 |

### 10.4 SpeakerNameText

| 속성 | 값 |
|------|-----|
| Anchor | Stretch-Top |
| Font Size | 24 |
| Alignment | Left |
| 상태 | 비활성화 (누적 방식에서 미사용) |

---

## 11. 코드 간소화 전후 비교

### 11.1 간소화 전

```csharp
private void SetupUILayout()
{
    // DialoguePanel RectTransform 설정 (약 20줄)
    // SpeakerNameText RectTransform 및 TextMeshProUGUI 설정 (약 15줄)
    // DialogueText RectTransform 및 TextMeshProUGUI 설정 (약 20줄)
    // ClickHint RectTransform 및 TextMeshProUGUI 설정 (약 20줄)
    // 총 약 75줄
}
```

### 11.2 간소화 후

```csharp
private void SetupUILayout()
{
    // SpeakerNameText는 누적 방식에서는 사용하지 않음 (숨기기)
    if (speakerNameText != null)
    {
        speakerNameText.gameObject.SetActive(false);
    }

    // ClickHint 텍스트 설정
    if (clickHint != null)
    {
        TextMeshProUGUI hintText = clickHint.GetComponent<TextMeshProUGUI>();
        if (hintText != null)
        {
            hintText.text = "클릭하여 계속...";
        }
    }
    // 총 약 15줄
}
```

---

## 12. 주의사항

1. **레이아웃 관리**: 모든 레이아웃과 폰트 설정은 Unity 에디터에서 관리합니다. 코드에서 설정하지 않습니다.
2. **씬 파일 동기화**: 코드를 수정할 때는 Unity 씬 파일의 설정도 함께 확인해야 합니다.
3. **SpeakerNameText**: 현재는 비활성화되어 있으나, 향후 개별 대사 표시 방식으로 변경 시 사용할 수 있습니다.
4. **폰트 에셋**: 한글을 제대로 표시하려면 한글을 지원하는 폰트 에셋을 사용해야 합니다 (예: NanumGothic SDF).

---

## 13. 향후 개선 가능 사항

1. **스크롤 기능**: 대사가 많아질 경우 스크롤 기능 추가
2. **대사 저장**: 완료된 대사를 저장하여 다시 확인할 수 있는 기능
3. **애니메이션**: 대화 패널 등장/퇴장 애니메이션 추가
4. **사운드**: 타이핑 효과음 추가


