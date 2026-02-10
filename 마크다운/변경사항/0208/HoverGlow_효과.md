# StepMother / Cake Hover 발광 효과 추가

## 작업일: 2026-02-08

---

## 1. 개요

StepMother와 Cake 오브젝트에 마우스를 올리면(Hover) 스프라이트가 밝게 빛나고, 마우스를 떼면 원래 색상으로 돌아오는 효과를 구현했습니다.

---

## 2. 생성된 파일

### `Assets/Scripts/Ryu/HoverGlow.cs` (신규)

**역할**: SpriteRenderer 기반 오브젝트의 마우스 Hover 발광 효과

**동작 원리**:
- `OnMouseEnter()`: 스프라이트 색상의 RGB를 `glowMultiplier`배로 곱하여 밝게 표시
- `OnMouseExit()`: 원래 색상으로 복원
- `OnDisable()`: 비활성화 시에도 원래 색상으로 안전하게 복원

**필수 컴포넌트** (`RequireComponent`):
- `SpriteRenderer` — 색상 변경 대상
- `BoxCollider2D` — 마우스 이벤트 감지용

**Inspector 설정 항목**:
| 항목 | 타입 | 기본값 | 설명 |
|------|------|--------|------|
| glowMultiplier | float | 1.5 | Hover 시 밝기 배율 (1보다 크면 밝아짐) |

**주요 메서드**:
```csharp
private void OnMouseEnter()   // Hover 시작 → 밝기 증가
private void OnMouseExit()    // Hover 종료 → 원래 색상 복원
private void OnDisable()      // 비활성화 시 색상 복원
```

---

## 3. 씬 변경사항 (Tutorial.unity)

### 컴포넌트 추가

| GameObject | instanceID | 추가된 컴포넌트 |
|------------|-----------|----------------|
| StepMother | 66614 | HoverGlow |
| Cake | 66622 | HoverGlow |

두 오브젝트 모두 기존에 `SpriteRenderer`와 `BoxCollider2D`를 이미 보유하고 있어, `HoverGlow`만 추가하면 즉시 동작합니다.

---

## 4. 동작 흐름

```
[기본 상태]
  스프라이트: 원래 색상

        ↓ 마우스 Hover (OnMouseEnter)

[발광 상태]
  스프라이트: RGB × glowMultiplier (기본 1.5배 밝기)

        ↓ 마우스 벗어남 (OnMouseExit)

[기본 상태]
  스프라이트: 원래 색상으로 복원
```

---

## 5. 디버그 로그

| 태그 | 내용 |
|------|------|
| `[HoverGlow]` | `{오브젝트명} Hover 시작` |
| `[HoverGlow]` | `{오브젝트명} Hover 종료` |

---

## 6. 버그 수정: 발광 효과 미동작

**문제**: Hover 이벤트는 감지되지만 (콘솔 로그 확인) 실제로 밝아지지 않음

**원인**: `Mathf.Min(..., 1f)`로 색상값을 1.0에 클램핑하고 있었음. SpriteRenderer의 기본 색상이 흰색 (1, 1, 1)이므로, 1.5를 곱해도 `Mathf.Min`에 의해 다시 1.0으로 잘려서 시각적 변화 없음.

```csharp
// 수정 전 (색상값이 1.0을 초과할 수 없음 → 흰색 스프라이트에서 변화 없음)
Mathf.Min(originalColor.r * glowMultiplier, 1f)

// 수정 후 (1.0 초과 허용 → 기본 셰이더가 밝게 렌더링)
originalColor.r * glowMultiplier
```

**해결**: `Mathf.Min` 클램핑 제거. Unity의 `Color` 구조체와 기본 스프라이트 셰이더는 1.0 초과 색상값을 지원하여 밝게 렌더링함.

---

## 7. 파일 구조

```
Assets/
└── Scripts/
    └── Ryu/
        ├── Global/
        │   └── ApiClient.cs
        ├── InputHandler.cs
        └── HoverGlow.cs              ← Hover 발광 효과 (신규)
```

