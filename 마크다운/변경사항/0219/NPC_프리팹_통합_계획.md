# NPC 프리팹 통합 계획 (2026-02-19)

## 목표

### 요구사항
- 여러 개의 개별 NPC 프리팹을 하나의 통합 프리팹으로 통합
- 스프라이트는 기본적으로 미지정 상태
- 클릭 시 선택될 이름은 Inspector에서 설정 가능
- 기존 프리팹 구조와 호환성 유지

### 현재 상황
- 프로젝트에 5개의 개별 NPC 프리팹이 존재:
  - `NPC_Sibling.prefab`
  - `NPC_Dog.prefab`
  - `NPC_Grandmother.prefab`
  - `NPC_NewFather.prefab`
  - `NPC_NewMother.prefab`
- 모든 프리팹이 동일한 구조를 가짐:
  - `Transform`
  - `SpriteRenderer` (스프라이트 미지정)
  - `BoxCollider2D`
  - `HoverGlow` 컴포넌트
  - `ClickableObject` 컴포넌트 (blockName 필드)
- `ClickableObject`는 클릭 시 `InputField`에 `@{blockName}`을 삽입하는 기능 제공

---

## 선택한 방법

### 통합 방식: 하나의 통합 프리팹 + Variant 시스템

**원리**: 
- 하나의 기본 프리팹(`NPC_Base.prefab`)을 생성
- 기존 개별 프리팹들을 이 기본 프리팹의 Variant로 변환
- 각 Variant에서 `ClickableObject`의 `blockName`과 `SpriteRenderer`의 스프라이트를 개별 설정

**장점**:
- ✅ 하나의 기본 프리팹으로 모든 NPC 관리
- ✅ Variant를 통해 각 NPC별 개별 설정 유지
- ✅ 공통 구조 변경 시 모든 Variant에 자동 반영
- ✅ 기존 프리팹 구조와 호환성 유지
- ✅ Inspector에서 직관적인 설정 가능

---

## 구현 계획

### 1. 통합 기본 프리팹 생성

**파일 위치**: `Assets/Assets/Prefabs/NPCs/NPC_Base.prefab`

**구조**:
- GameObject: `NPC_Base`
- Components:
  - `Transform`
  - `SpriteRenderer` (스프라이트: None)
  - `BoxCollider2D` (Size: 0.0001, 0.0001)
  - `HoverGlow` (glowMultiplier: 1.5)
  - `ClickableObject` (blockName: 빈 문자열)

**설정 사항**:
- `SpriteRenderer.sprite`: None (기본값)
- `ClickableObject.blockName`: 빈 문자열 (Inspector에서 설정)
- `BoxCollider2D`: 기존과 동일한 크기 유지

---

### 2. 기존 프리팹을 Variant로 변환

**변환 대상**:
1. `NPC_Sibling.prefab` → `NPC_Base` Variant
2. `NPC_Dog.prefab` → `NPC_Base` Variant
3. `NPC_Grandmother.prefab` → `NPC_Base` Variant
4. `NPC_NewFather.prefab` → `NPC_Base` Variant
5. `NPC_NewMother.prefab` → `NPC_Base` Variant

**변환 절차**:
1. Unity Editor에서 각 기존 프리팹 선택
2. `Prefab` 메뉴 → `Unpack Prefab Completely` (임시)
3. `NPC_Base` 프리팹을 부모로 설정하여 Variant 생성
4. 각 Variant에서 `ClickableObject.blockName` 설정:
   - `NPC_Sibling`: "Sibling" 또는 "형제"
   - `NPC_Dog`: "Dog" 또는 "개"
   - `NPC_Grandmother`: "Grandmother" 또는 "할머니"
   - `NPC_NewFather`: "NewFather" 또는 "새아버지"
   - `NPC_NewMother`: "NewMother" 또는 "새엄마"

**주의사항**:
- 기존 씬에 배치된 프리팹 인스턴스는 자동으로 새 Variant로 연결됨
- Variant 생성 후 기존 프리팹 파일은 삭제 가능 (백업 권장)

---

### 3. Inspector 설정 가이드

**각 Variant에서 설정할 항목**:

1. **ClickableObject 컴포넌트**
   - `Block Name`: 클릭 시 입력 필드에 삽입될 이름
   - 예: "Sibling", "Dog", "Grandmother" 등

2. **SpriteRenderer 컴포넌트**
   - `Sprite`: NPC 스프라이트 (기본값: None)
   - 필요 시 각 Variant에서 개별 스프라이트 할당

3. **기타 설정**
   - `HoverGlow.glowMultiplier`: 필요 시 조정 (기본값: 1.5)
   - `BoxCollider2D`: 필요 시 크기 조정

---

## 적용사항

### 완료된 작업 (2026-02-19)

#### Phase 1: 기본 프리팹 생성 ✅
- [x] `NPC_Base.prefab` 생성
  - **위치**: `Assets/Assets/Prefabs/NPCs/NPC_Base.prefab`
  - Unity MCP를 통해 GameObject 생성 및 프리팹 저장 완료
- [x] `Transform` 컴포넌트 확인
- [x] `SpriteRenderer` 컴포넌트 추가 (스프라이트: None)
- [x] `BoxCollider2D` 컴포넌트 추가 (Size: 0.0001, 0.0001)
- [x] `HoverGlow` 컴포넌트 추가 (glowMultiplier: 1.5)
- [x] `ClickableObject` 컴포넌트 추가 (blockName: 빈 문자열)
- [x] 기본 프리팹 저장 및 확인

#### 추가 작업 ✅
- [x] Editor 스크립트 생성
  - **파일**: `Assets/Scripts/Editor/CreateNPCVariants.cs`
  - **기능**: Unity Editor 메뉴에서 `Tools` → `NPC` → `Create NPC Variants`로 접근 가능
  - **용도**: Variant 생성 자동화 지원 (참고: Unity MCP로는 Prefab Variant 직접 생성 불가)
- [x] 가이드 문서 작성
  - **파일**: `마크다운/변경사항/0219/NPC_Variant_생성_가이드.md`
  - **내용**: Unity Editor에서 수동으로 Variant 생성하는 방법 안내

### 진행 중인 작업

#### Phase 2: Variant 생성 (Unity Editor에서 수동 작업 필요)
- [ ] `NPC_Sibling` Variant 생성 및 blockName 설정
- [ ] `NPC_Dog` Variant 생성 및 blockName 설정
- [ ] `NPC_Grandmother` Variant 생성 및 blockName 설정
- [ ] `NPC_NewFather` Variant 생성 및 blockName 설정
- [ ] `NPC_NewMother` Variant 생성 및 blockName 설정

**참고**: Unity MCP의 제한으로 인해 Prefab Variant는 Unity Editor에서 수동으로 생성해야 합니다. 자세한 방법은 `NPC_Variant_생성_가이드.md`를 참고하세요.

### 미완료 작업

#### Phase 3: 기존 프리팹 처리
- [ ] 씬에 배치된 기존 프리팹 인스턴스 확인
- [ ] 인스턴스가 새 Variant로 올바르게 연결되었는지 확인
- [ ] 기존 프리팹 파일 백업 (선택사항)
- [ ] 기존 프리팹 파일 삭제 또는 보관 결정

#### Phase 4: 테스트 및 검증
- [ ] 각 Variant의 blockName이 올바르게 설정되었는지 확인
- [ ] 클릭 시 InputField에 올바른 이름이 삽입되는지 테스트
- [ ] HoverGlow 효과가 정상 작동하는지 확인
- [ ] 스프라이트 할당이 정상 작동하는지 확인 (스프라이트가 있는 경우)
- [ ] 씬에 배치된 모든 NPC 인스턴스가 정상 작동하는지 확인

---

## 작업 체크리스트

### Phase 1: 기본 프리팹 생성 ✅
- [x] `NPC_Base.prefab` 생성
- [x] `Transform` 컴포넌트 확인
- [x] `SpriteRenderer` 컴포넌트 추가 (스프라이트: None)
- [x] `BoxCollider2D` 컴포넌트 추가 (Size: 0.0001, 0.0001)
- [x] `HoverGlow` 컴포넌트 추가 (glowMultiplier: 1.5)
- [x] `ClickableObject` 컴포넌트 추가 (blockName: 빈 문자열)
- [x] 기본 프리팹 저장 및 확인

### Phase 2: Variant 생성
- [ ] `NPC_Sibling` Variant 생성 및 blockName 설정
- [ ] `NPC_Dog` Variant 생성 및 blockName 설정
- [ ] `NPC_Grandmother` Variant 생성 및 blockName 설정
- [ ] `NPC_NewFather` Variant 생성 및 blockName 설정
- [ ] `NPC_NewMother` Variant 생성 및 blockName 설정

### Phase 3: 기존 프리팹 처리
- [ ] 씬에 배치된 기존 프리팹 인스턴스 확인
- [ ] 인스턴스가 새 Variant로 올바르게 연결되었는지 확인
- [ ] 기존 프리팹 파일 백업 (선택사항)
- [ ] 기존 프리팹 파일 삭제 또는 보관 결정

### Phase 4: 테스트 및 검증
- [ ] 각 Variant의 blockName이 올바르게 설정되었는지 확인
- [ ] 클릭 시 InputField에 올바른 이름이 삽입되는지 테스트
- [ ] HoverGlow 효과가 정상 작동하는지 확인
- [ ] 스프라이트 할당이 정상 작동하는지 확인 (스프라이트가 있는 경우)
- [ ] 씬에 배치된 모든 NPC 인스턴스가 정상 작동하는지 확인

---

## 기술 세부사항

### 생성된 파일

1. **NPC_Base.prefab**
   - **위치**: `Assets/Assets/Prefabs/NPCs/NPC_Base.prefab`
   - **용도**: 모든 NPC의 기본 프리팹
   - **구조**: Transform, SpriteRenderer, BoxCollider2D, HoverGlow, ClickableObject

2. **CreateNPCVariants.cs**
   - **위치**: `Assets/Scripts/Editor/CreateNPCVariants.cs`
   - **용도**: Unity Editor에서 Variant 생성 자동화 지원
   - **접근**: `Tools` → `NPC` → `Create NPC Variants` 메뉴
   - **주의**: Unity MCP 제한으로 완전 자동화는 불가능하며, Unity Editor에서 수동 작업 필요

3. **NPC_Variant_생성_가이드.md**
   - **위치**: `마크다운/변경사항/0219/NPC_Variant_생성_가이드.md`
   - **용도**: Unity Editor에서 Variant 생성하는 방법 안내

### 컴포넌트 상세

#### ClickableObject 컴포넌트
- **위치**: `Assets/Scripts/Ryu/Gameplay/ClickableObject.cs`
- **주요 기능**:
  - `blockName` 필드: Inspector에서 설정 가능
  - 클릭 시 `InputHandler.AddBlockToInput(blockName)` 호출
  - `blockName`이 비어있으면 `gameObject.name` 사용

#### HoverGlow 컴포넌트
- **위치**: `Assets/Scripts/Ryu/Gameplay/HoverGlow.cs`
- **기능**: 마우스 호버 시 발광 효과 제공
- **설정**: `glowMultiplier` (기본값: 1.5)

#### BoxCollider2D
- **용도**: 클릭 감지 영역
- **크기**: 0.0001 x 0.0001 (기본값, 필요 시 조정)

---

## 예상 결과

### 통합 전
```
Assets/Assets/Prefabs/NPCs/
├── NPC_Sibling.prefab
├── NPC_Dog.prefab
├── NPC_Grandmother.prefab
├── NPC_NewFather.prefab
└── NPC_NewMother.prefab
```

### 통합 후
```
Assets/Assets/Prefabs/NPCs/
├── NPC_Base.prefab (기본 프리팹)
├── NPC_Sibling Variant.prefab
├── NPC_Dog Variant.prefab
├── NPC_Grandmother Variant.prefab
├── NPC_NewFather Variant.prefab
└── NPC_NewMother Variant.prefab
```

### 장점
- ✅ 하나의 기본 프리팹으로 모든 NPC 관리
- ✅ 공통 구조 변경 시 모든 Variant에 자동 반영
- ✅ 각 NPC별 개별 설정은 Variant에서 관리
- ✅ Inspector에서 직관적인 설정 가능
- ✅ 코드 변경 없이 기존 시스템과 호환

---

## 주의사항

1. **기존 프리팹 백업**: Variant 생성 전 기존 프리팹 파일을 백업하는 것을 권장합니다.

2. **씬 인스턴스 확인**: Variant 생성 후 씬에 배치된 모든 NPC 인스턴스가 올바르게 연결되었는지 확인해야 합니다.

3. **blockName 설정**: 각 Variant의 `ClickableObject.blockName`이 올바르게 설정되었는지 확인해야 합니다.

4. **스프라이트 할당**: 스프라이트는 기본적으로 None이지만, 필요 시 각 Variant에서 개별 할당 가능합니다.

5. **Variant 오버라이드**: Variant에서 설정한 값은 기본 프리팹의 값을 오버라이드합니다. 기본 프리팹을 수정하면 Variant에 영향을 줄 수 있으므로 주의가 필요합니다.

---

## 참고 자료

- Unity Prefab Variant 문서: https://docs.unity3d.com/Manual/PrefabVariants.html
- `ClickableObject` 스크립트: `Assets/Scripts/Ryu/Gameplay/ClickableObject.cs`
- `HoverGlow` 스크립트: `Assets/Scripts/Ryu/Gameplay/HoverGlow.cs`
- Variant 생성 가이드: `마크다운/변경사항/0219/NPC_Variant_생성_가이드.md`
- Editor 스크립트: `Assets/Scripts/Editor/CreateNPCVariants.cs`

---

## 변경 이력

- 2026-02-19: 초안 작성
- 2026-02-19: Phase 1 완료 (NPC_Base 프리팹 생성, Editor 스크립트 및 가이드 문서 작성)

