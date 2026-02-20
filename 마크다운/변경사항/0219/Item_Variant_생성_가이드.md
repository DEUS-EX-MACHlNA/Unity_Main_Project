# Item Variant 생성 가이드

## 현재 상태

✅ **완료된 작업**:
- `Item_Base.prefab` 생성 완료
- 모든 필요한 컴포넌트 추가 완료 (SpriteRenderer, BoxCollider2D, HoverGlow, ClickableObject)
- BoxCollider2D size: 0.0001, 0.0001 (스프라이트 할당 시 자동 조정)
- HoverGlow glowMultiplier: 1.5
- ClickableObject blockName: 빈 문자열
- SpriteRenderer sortingOrder: 1

## Unity Editor에서 Variant 생성 방법

Unity MCP로는 Prefab Variant를 직접 생성할 수 없으므로, Unity Editor에서 수동으로 작업해야 합니다.

### 방법 1: Unity Editor에서 직접 생성 (권장)

1. **Project 창에서 `Item_Base.prefab` 선택**
   - 경로: `Assets/Assets/Prefabs/Items/Item_Base.prefab`

2. **우클릭 → "Create Prefab Variant" 선택**
   - 또는 `Assets` 메뉴 → `Create` → `Prefab Variant`

3. **생성된 Variant 이름 변경**
   - 예: `Item_Base Variant` → `Item_BrassKey`

4. **Variant 선택 후 Inspector에서 설정**
   - `ClickableObject` 컴포넌트의 `Block Name` 필드에 이름 입력
     - `Item_BrassKey`: "BrassKey"
     - `Item_EarlGreyTea`: "EarlGreyTea"
     - `Item_Hole`: "Hole"
     - `Item_OilBottle`: "OilBottle"
     - `Item_RealFamilyPhoto`: "RealFamilyPhoto"
     - `Item_SiblingsToy`: "SiblingsToy"
     - `Item_SilverLighter`: "SilverLighter"
     - `Item_SleepingPill`: "SleepingPill"
   - `SpriteRenderer` 컴포넌트의 `Sprite` 필드에 기존 스프라이트 할당 (있는 경우)

5. **반복하여 8개의 Variant 생성**

### 방법 2: 기존 프리팹을 Variant로 변환

1. **씬에 `Item_Base` 프리팹 인스턴스 생성**

2. **Inspector에서 설정**
   - `ClickableObject.blockName` 설정 (예: "BrassKey")
   - `SpriteRenderer.sprite` 설정 (기존 프리팹에 스프라이트가 있었던 경우)

3. **해당 GameObject를 선택한 상태에서**
   - `GameObject` 메뉴 → `Prefab` → `Save As Prefab Variant`
   - 또는 Project 창에서 `Item_Base`를 드래그하여 부모로 설정

4. **저장 경로 지정**
   - `Assets/Assets/Prefabs/Items/Item_BrassKey.prefab` (기존 프리팹 덮어쓰기)

5. **기존 프리팹 파일 삭제 또는 백업**

## Editor 스크립트 사용 (선택사항)

`Assets/Scripts/Editor/CreateItemVariants.cs` 스크립트를 사용할 수 있습니다.

**사용 방법**:
1. Unity Editor에서 `Tools` 메뉴 → `Item` → `Create Item Variants` 선택
2. 창이 열리면 "Create All Variants" 버튼 클릭
3. **주의**: 생성된 프리팹을 Unity Editor에서 수동으로 Variant로 변환해야 합니다.
4. **주의**: 스프라이트는 자동으로 복사되지만, Variant로 변환 후 확인이 필요합니다.

## 각 Variant 설정 체크리스트

각 Variant 생성 후 다음을 확인하세요:

- [ ] `ClickableObject.blockName`이 올바르게 설정되었는지
- [ ] `SpriteRenderer.sprite`가 기존 프리팹과 동일한지 (스프라이트가 있었던 경우)
- [ ] `SpriteRenderer.sortingOrder`가 1인지 (기본값)
- [ ] `HoverGlow.glowMultiplier`가 1.5인지 (기본값)
- [ ] `BoxCollider2D.size`가 스프라이트 크기에 맞게 자동 조정되었는지 (스프라이트 할당 시)

## 기존 프리팹 처리

Variant 생성 후 기존 프리팹 파일을 어떻게 처리할지 결정하세요:

1. **백업 후 삭제** (권장)
   - 기존 프리팹을 `_backup` 접미사로 백업
   - 씬에 배치된 인스턴스가 새 Variant로 연결되었는지 확인 후 삭제

2. **보관**
   - 참고용으로 보관
   - 새 Variant만 사용

## 문제 해결

### Variant가 생성되지 않는 경우
- `Item_Base.prefab`이 올바른 경로에 있는지 확인
- Unity Editor를 재시작해보세요

### 씬에 배치된 프리팹이 작동하지 않는 경우
- 씬의 프리팹 인스턴스를 선택
- Inspector에서 "Open Prefab" 버튼 클릭
- 새 Variant로 연결되었는지 확인

### blockName이 설정되지 않는 경우
- Variant를 선택하고 Inspector에서 직접 확인
- `ClickableObject` 컴포넌트의 `Block Name` 필드에 값이 있는지 확인

### 스프라이트가 표시되지 않는 경우
- Variant를 선택하고 Inspector에서 확인
- `SpriteRenderer` 컴포넌트의 `Sprite` 필드에 스프라이트가 할당되었는지 확인
- 기존 프리팹에서 스프라이트를 확인하고 동일하게 할당

### BoxCollider2D 크기가 맞지 않는 경우
- `HoverGlow` 컴포넌트가 스프라이트 할당 시 자동으로 크기를 조정합니다
- 스프라이트가 할당된 후 씬에서 확인해보세요
- 필요 시 수동으로 크기 조정 가능

