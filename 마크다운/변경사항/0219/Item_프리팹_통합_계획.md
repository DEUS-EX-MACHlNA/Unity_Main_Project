# Item 프리팹 통합 계획 (2026-02-19)

## 목표

### 요구사항
- 여러 개의 개별 Item 프리팹을 하나의 통합 프리팹으로 통합
- 스프라이트는 기본적으로 미지정 상태
- 클릭 시 선택될 이름은 Inspector에서 설정 가능
- 기존 프리팹 구조와 호환성 유지

### 현재 상황
- 프로젝트에 8개의 개별 Item 프리팹이 존재:
  - `Item_BrassKey.prefab`
  - `Item_EarlGreyTea.prefab`
  - `Item_Hole.prefab`
  - `Item_OilBottle.prefab`
  - `Item_RealFamilyPhoto.prefab`
  - `Item_SiblingsToy.prefab`
  - `Item_SilverLighter.prefab`
  - `Item_SleepingPill.prefab`
- 모든 프리팹이 동일한 구조를 가짐:
  - `Transform`
  - `SpriteRenderer` (일부는 스프라이트 할당됨)
  - `BoxCollider2D` (스프라이트 크기에 맞춰 설정됨)
  - `HoverGlow` 컴포넌트 (glowMultiplier: 1.5)
  - `ClickableObject` 컴포넌트 (blockName: 빈 문자열)
- `ClickableObject`는 클릭 시 `InputField`에 `@{blockName}`을 삽입하는 기능 제공

---

## 선택한 방법

### 통합 방식: 하나의 통합 프리팹 + Variant 시스템

**원리**: 
- 하나의 기본 프리팹(`Item_Base.prefab`)을 생성
- 기존 개별 프리팹들을 이 기본 프리팹의 Variant로 변환
- 각 Variant에서 `ClickableObject`의 `blockName`과 `SpriteRenderer`의 스프라이트를 개별 설정

**장점**:
- ✅ 하나의 기본 프리팹으로 모든 Item 관리
- ✅ Variant를 통해 각 Item별 개별 설정 유지
- ✅ 공통 구조 변경 시 모든 Variant에 자동 반영
- ✅ 기존 프리팹 구조와 호환성 유지
- ✅ Inspector에서 직관적인 설정 가능

---

## 구현 계획

### 1. 통합 기본 프리팹 생성

**파일 위치**: `Assets/Assets/Prefabs/Items/Item_Base.prefab`

**구조**:
- GameObject: `Item_Base`
- Components:
  - `Transform`
  - `SpriteRenderer` (스프라이트: None, sortingOrder: 1)
  - `BoxCollider2D` (Size: 0.0001, 0.0001 - 기본값, 스프라이트 할당 시 자동 조정)
  - `HoverGlow` (glowMultiplier: 1.5)
  - `ClickableObject` (blockName: 빈 문자열)

**설정 사항**:
- `SpriteRenderer.sprite`: None (기본값)
- `SpriteRenderer.sortingOrder`: 1 (기본값)
- `ClickableObject.blockName`: 빈 문자열 (Inspector에서 설정)
- `BoxCollider2D`: 기본값 0.0001, 0.0001 (스프라이트 할당 시 HoverGlow가 자동으로 크기 조정)

---

### 2. 기존 프리팹을 Variant로 변환

**변환 대상**:
1. `Item_BrassKey.prefab` → `Item_Base` Variant
2. `Item_EarlGreyTea.prefab` → `Item_Base` Variant
3. `Item_Hole.prefab` → `Item_Base` Variant
4. `Item_OilBottle.prefab` → `Item_Base` Variant
5. `Item_RealFamilyPhoto.prefab` → `Item_Base` Variant
6. `Item_SiblingsToy.prefab` → `Item_Base` Variant
7. `Item_SilverLighter.prefab` → `Item_Base` Variant
8. `Item_SleepingPill.prefab` → `Item_Base` Variant

**변환 절차**:
1. Unity Editor에서 `Tools` → `Item` → `Create Item Base Prefab` 실행하여 기본 프리팹 생성
2. Unity Editor에서 `Tools` → `Item` → `Create Item Variants` 실행하여 Variant 생성
3. 각 Variant에서 `ClickableObject.blockName` 설정:
   - `Item_BrassKey`: "BrassKey"
   - `Item_EarlGreyTea`: "EarlGreyTea"
   - `Item_Hole`: "Hole"
   - `Item_OilBottle`: "OilBottle"
   - `Item_RealFamilyPhoto`: "RealFamilyPhoto"
   - `Item_SiblingsToy`: "SiblingsToy"
   - `Item_SilverLighter`: "SilverLighter"
   - `Item_SleepingPill`: "SleepingPill"
4. 각 Variant에서 기존 스프라이트 재할당 (있는 경우)

**주의사항**:
- 기존 씬에 배치된 프리팹 인스턴스는 자동으로 새 Variant로 연결됨
- Variant 생성 후 기존 프리팹 파일은 백업 후 삭제됨
- 스프라이트가 할당된 Item의 경우 Variant에서 스프라이트가 자동으로 재할당됨

---

### 3. Inspector 설정 가이드

**각 Variant에서 설정할 항목**:

1. **ClickableObject 컴포넌트**
   - `Block Name`: 클릭 시 입력 필드에 삽입될 이름
   - 예: "BrassKey", "EarlGreyTea", "Hole" 등

2. **SpriteRenderer 컴포넌트**
   - `Sprite`: Item 스프라이트 (기본값: None)
   - 기존 프리팹에 스프라이트가 있었던 경우 Variant에서 자동 재할당됨
   - `Sorting Order`: 1 (기본값, 필요 시 조정)

3. **BoxCollider2D 컴포넌트**
   - 스프라이트 할당 시 `HoverGlow` 컴포넌트가 자동으로 크기 조정
   - 필요 시 수동으로 크기 조정 가능

4. **기타 설정**
   - `HoverGlow.glowMultiplier`: 필요 시 조정 (기본값: 1.5)
   - `Transform`: 위치, 회전, 스케일은 씬 인스턴스에서 개별 설정

---

## 적용사항

### 완료된 작업 (2026-02-19)

#### Phase 1: 기본 프리팹 생성 스크립트 ✅
- [x] `CreateItemBase.cs` Editor 스크립트 생성
  - **위치**: `Assets/Scripts/Editor/CreateItemBase.cs`
  - **기능**: Unity Editor 메뉴에서 `Tools` → `Item` → `Create Item Base Prefab`로 접근 가능
  - **용도**: Item_Base 프리팹 자동 생성

#### Phase 2: Variant 생성 스크립트 ✅
- [x] `CreateItemVariants.cs` Editor 스크립트 생성
  - **위치**: `Assets/Scripts/Editor/CreateItemVariants.cs`
  - **기능**: Unity Editor 메뉴에서 `Tools` → `Item` → `Create Item Variants`로 접근 가능
  - **용도**: 모든 Item 프리팹을 Variant로 자동 변환
  - **특징**: 
    - 기존 스프라이트 자동 재할당
    - blockName 자동 설정
    - 기존 프리팹 백업 생성

### 진행 중인 작업

#### Phase 3: Unity Editor에서 실행 필요
- [ ] Unity Editor에서 `Tools` → `Item` → `Create Item Base Prefab` 실행
- [ ] Unity Editor에서 `Tools` → `Item` → `Create Item Variants` 실행
- [ ] 생성된 Variant 확인 및 검증

### 미완료 작업

#### Phase 4: 기존 프리팹 처리
- [ ] 씬에 배치된 기존 프리팹 인스턴스 확인
- [ ] 인스턴스가 새 Variant로 올바르게 연결되었는지 확인
- [ ] 백업 파일 확인 및 정리

#### Phase 5: 테스트 및 검증
- [ ] 각 Variant의 blockName이 올바르게 설정되었는지 확인
- [ ] 클릭 시 InputField에 올바른 이름이 삽입되는지 테스트
- [ ] HoverGlow 효과가 정상 작동하는지 확인
- [ ] 스프라이트가 올바르게 할당되었는지 확인
- [ ] BoxCollider2D 크기가 스프라이트에 맞게 조정되었는지 확인
- [ ] 씬에 배치된 모든 Item 인스턴스가 정상 작동하는지 확인

---

## 작업 체크리스트

### Phase 1: 기본 프리팹 생성 스크립트 ✅
- [x] `CreateItemBase.cs` Editor 스크립트 생성
- [x] `Item_Base` GameObject 생성 로직 구현
- [x] 모든 컴포넌트 추가 로직 구현
- [x] 프리팹 저장 로직 구현

### Phase 2: Variant 생성 스크립트 ✅
- [x] `CreateItemVariants.cs` Editor 스크립트 생성
- [x] 8개 Item 프리팹 변환 로직 구현
- [x] blockName 자동 설정 로직 구현
- [x] 스프라이트 자동 재할당 로직 구현
- [x] 백업 생성 로직 구현

### Phase 3: Unity Editor에서 실행
- [ ] Unity Editor 열기
- [ ] `Tools` → `Item` → `Create Item Base Prefab` 실행
- [ ] `Item_Base.prefab` 생성 확인
- [ ] `Tools` → `Item` → `Create Item Variants` 실행
- [ ] 8개 Variant 생성 확인

### Phase 4: 기존 프리팹 처리
- [ ] 씬에 배치된 기존 프리팹 인스턴스 확인
- [ ] 인스턴스가 새 Variant로 올바르게 연결되었는지 확인
- [ ] 백업 파일 확인 및 정리

### Phase 5: 테스트 및 검증
- [ ] 각 Variant의 blockName이 올바르게 설정되었는지 확인
- [ ] 클릭 시 InputField에 올바른 이름이 삽입되는지 테스트
- [ ] HoverGlow 효과가 정상 작동하는지 확인
- [ ] 스프라이트가 올바르게 할당되었는지 확인
- [ ] BoxCollider2D 크기가 스프라이트에 맞게 조정되었는지 확인
- [ ] 씬에 배치된 모든 Item 인스턴스가 정상 작동하는지 확인

---

## 기술 세부사항

### 생성된 파일

1. **CreateItemBase.cs**
   - **위치**: `Assets/Scripts/Editor/CreateItemBase.cs`
   - **용도**: Item_Base 프리팹 자동 생성
   - **접근**: `Tools` → `Item` → `Create Item Base Prefab` 메뉴
   - **기능**:
     - GameObject 생성
     - 모든 컴포넌트 추가 (SpriteRenderer, BoxCollider2D, HoverGlow, ClickableObject)
     - 프리팹으로 저장

2. **CreateItemVariants.cs**
   - **위치**: `Assets/Scripts/Editor/CreateItemVariants.cs`
   - **용도**: 모든 Item 프리팹을 Variant로 자동 변환
   - **접근**: `Tools` → `Item` → `Create Item Variants` 메뉴
   - **기능**:
     - Base 프리팹 로드
     - 8개 Item 프리팹을 Variant로 변환
     - blockName 자동 설정
     - 스프라이트 자동 재할당
     - 기존 프리팹 백업 생성

3. **Item_Base.prefab** (생성 예정)
   - **위치**: `Assets/Assets/Prefabs/Items/Item_Base.prefab`
   - **용도**: 모든 Item의 기본 프리팹
   - **구조**: Transform, SpriteRenderer, BoxCollider2D, HoverGlow, ClickableObject

### 컴포넌트 상세

#### ClickableObject 컴포넌트
- **위치**: `Assets/Scripts/Ryu/Gameplay/ClickableObject.cs`
- **주요 기능**:
  - `blockName` 필드: Inspector에서 설정 가능
  - 클릭 시 `InputHandler.AddBlockToInput(blockName)` 호출
  - `blockName`이 비어있으면 `gameObject.name` 사용

#### HoverGlow 컴포넌트
- **위치**: `Assets/Scripts/Ryu/Gameplay/HoverGlow.cs`
- **기능**: 
  - 마우스 호버 시 발광 효과 제공
  - 스프라이트 할당 시 BoxCollider2D 크기 자동 조정
- **설정**: `glowMultiplier` (기본값: 1.5)

#### BoxCollider2D
- **용도**: 클릭 감지 영역
- **크기**: 
  - 기본값: 0.0001 x 0.0001
  - 스프라이트 할당 시 `HoverGlow`가 자동으로 스프라이트 크기에 맞춰 조정

#### SpriteRenderer
- **설정**:
  - `sprite`: None (기본값)
  - `sortingOrder`: 1 (기본값, NPC는 0)

---

## 예상 결과

### 통합 전
```
Assets/Assets/Prefabs/Items/
├── Item_BrassKey.prefab
├── Item_EarlGreyTea.prefab
├── Item_Hole.prefab
├── Item_OilBottle.prefab
├── Item_RealFamilyPhoto.prefab
├── Item_SiblingsToy.prefab
├── Item_SilverLighter.prefab
└── Item_SleepingPill.prefab
```

### 통합 후
```
Assets/Assets/Prefabs/Items/
├── Item_Base.prefab (기본 프리팹)
├── Item_BrassKey.prefab (Variant)
├── Item_EarlGreyTea.prefab (Variant)
├── Item_Hole.prefab (Variant)
├── Item_OilBottle.prefab (Variant)
├── Item_RealFamilyPhoto.prefab (Variant)
├── Item_SiblingsToy.prefab (Variant)
├── Item_SilverLighter.prefab (Variant)
└── Item_SleepingPill.prefab (Variant)
```

### 장점
- ✅ 하나의 기본 프리팹으로 모든 Item 관리
- ✅ 공통 구조 변경 시 모든 Variant에 자동 반영
- ✅ 각 Item별 개별 설정은 Variant에서 관리
- ✅ Inspector에서 직관적인 설정 가능
- ✅ 코드 변경 없이 기존 시스템과 호환

---

## NPC 프리팹 통합과의 차이점

### 공통점
- 동일한 컴포넌트 구조 (Transform, SpriteRenderer, BoxCollider2D, HoverGlow, ClickableObject)
- 동일한 통합 방식 (Base 프리팹 + Variant 시스템)
- 동일한 설정 방법 (Inspector에서 blockName 설정)

### 차이점
1. **스프라이트 할당**
   - NPC: 기본적으로 스프라이트 미지정
   - Item: 일부 프리팹에 스프라이트가 이미 할당되어 있음 → Variant에서 자동 재할당

2. **BoxCollider2D 크기**
   - NPC: 고정 크기 (0.0001, 0.0001)
   - Item: 스프라이트 크기에 맞춰 자동 조정 (HoverGlow 기능 활용)

3. **Sorting Order**
   - NPC: 0 (기본값)
   - Item: 1 (기본값, 스프라이트 레이어링)

4. **프리팹 개수**
   - NPC: 5개
   - Item: 8개

---

## 주의사항

1. **기존 프리팹 백업**: Variant 생성 전 기존 프리팹 파일이 자동으로 백업됩니다 (`_backup` 접미사).

2. **씬 인스턴스 확인**: Variant 생성 후 씬에 배치된 모든 Item 인스턴스가 올바르게 연결되었는지 확인해야 합니다.

3. **blockName 설정**: 각 Variant의 `ClickableObject.blockName`이 스크립트에 의해 자동으로 설정됩니다.

4. **스프라이트 재할당**: 기존 프리팹에 스프라이트가 있었던 경우, Variant에서 스프라이트가 자동으로 재할당됩니다.

5. **BoxCollider2D 자동 조정**: `HoverGlow` 컴포넌트가 스프라이트 할당 시 BoxCollider2D 크기를 자동으로 조정합니다. 필요 시 수동으로 조정 가능합니다.

6. **Variant 오버라이드**: Variant에서 설정한 값은 기본 프리팹의 값을 오버라이드합니다. 기본 프리팹을 수정하면 Variant에 영향을 줄 수 있으므로 주의가 필요합니다.

7. **Transform 설정**: 위치, 회전, 스케일은 프리팹 레벨이 아닌 씬 인스턴스에서 개별 설정됩니다.

8. **Unity Editor 실행 필요**: 스크립트는 Unity Editor에서 실행해야 합니다. Unity MCP로는 직접 실행할 수 없습니다.

---

## 참고 자료

- Unity Prefab Variant 문서: https://docs.unity3d.com/Manual/PrefabVariants.html
- `ClickableObject` 스크립트: `Assets/Scripts/Ryu/Gameplay/ClickableObject.cs`
- `HoverGlow` 스크립트: `Assets/Scripts/Ryu/Gameplay/HoverGlow.cs`
- NPC 프리팹 통합 계획: `마크다운/변경사항/0219/NPC_프리팹_통합_계획.md`
- NPC Variant 생성 가이드: `마크다운/변경사항/0219/NPC_Variant_생성_가이드.md` (참고용)

---

## 변경 이력

- 2026-02-19: 초안 작성
- 2026-02-19: Editor 스크립트 생성 완료 (CreateItemBase.cs, CreateItemVariants.cs)
