# NPC 및 아이템 Prefab 배치 가이드

이 가이드는 NPC와 아이템 Prefab을 씬에 배치하는 방법을 초보자 기준으로 상세하게 설명합니다.

## 목차

1. [NPC Prefab 배치](#1-npc-prefab-배치)
2. [아이템 Prefab 배치](#2-아이템-prefab-배치)
3. [씬별 배치 목록](#3-씬별-배치-목록)
4. [문제 해결](#4-문제-해결)

---

## 1. NPC Prefab 배치

### 1-1. NPC Prefab 위치 확인

1. **Project 창 열기**
   - Unity 에디터 하단의 `Project` 탭 클릭
   - 또는 `Window` → `General` → `Project`

2. **NPC Prefab 폴더로 이동**
   - Project 창에서 다음 경로로 이동:
     ```
     Assets → Assets → Prefabs → NPCs
     ```
   - 다음 NPC Prefab들이 보여야 합니다:
     - `NPC_NewMother.prefab` (새엄마)
     - `NPC_NewFather.prefab` (새아빠)
     - `NPC_Sibling.prefab` (동생)
     - `NPC_Dog.prefab` (강아지)
     - `NPC_Grandmother.prefab` (할머니)

### 1-2. NPC Prefab을 씬에 배치

#### 방법 1: 드래그 앤 드롭 (권장)

1. **Project 창에서 NPC Prefab 선택**
   - 예: `NPC_NewMother.prefab` 클릭

2. **Hierarchy 창으로 드래그**
   - Project 창의 NPC Prefab을 마우스로 클릭하고 드래그
   - Hierarchy 창으로 이동
   - Hierarchy 창에서 마우스 버튼 놓기

3. **배치 확인**
   - Hierarchy 창에 NPC 이름이 추가되었는지 확인
   - 예: `NPC_NewMother` 또는 `NPC_NewMother (1)`


### 1-3. NPC 위치 조정

1. **Hierarchy에서 NPC 선택**
   - Hierarchy 창에서 방금 추가한 NPC 클릭
   - 예: `NPC_NewMother`

2. **Scene 뷰 확인**
   - Scene 뷰에서 NPC가 보이는지 확인
   - NPC가 보이지 않으면 Scene 뷰를 이동/확대

3. **Transform 위치 조정**
   - Inspector 창에서 `Transform` 컴포넌트 확인
   - `Position` 값을 조정:
     - `X`: 좌우 위치
     - `Y`: 상하 위치
     - `Z`: 깊이 (보통 0 유지)
   - 예: `Position (2, 0, 0)` → NPC를 오른쪽으로 이동

4. **Scene 뷰에서 직접 이동**
   - Scene 뷰에서 NPC 선택
   - 화살표 모양의 이동 도구 사용 (단축키: `W`)
   - 화살표를 드래그하여 위치 조정

### 1-4. NPC Sprite 설정

1. **NPC 선택**
   - Hierarchy에서 NPC GameObject 선택

2. **SpriteRenderer 컴포넌트 확인**
   - Inspector 창에서 `Sprite Renderer` 컴포넌트 찾기
   - `Sprite` 필드 확인 (현재 `None (Sprite)`로 표시될 수 있음)

3. **Sprite 이미지 준비**
   - `Assets/Assets/Character/` 폴더에 NPC 이미지가 있는지 확인
   - 이미지가 없다면 준비 필요

4. **Sprite 할당**
   - 방법 1: Project 창에서 Sprite 이미지를 찾아 `Sprite` 필드로 드래그 앤 드롭
   - 방법 2: `Sprite` 필드 옆의 원형 아이콘 클릭 → Sprite 이미지 선택

### 1-5. ClickableObject 설정 확인

1. **NPC 선택**
   - Hierarchy에서 NPC GameObject 선택

2. **ClickableObject 컴포넌트 확인**
   - Inspector 창에서 `Clickable Object` 컴포넌트 찾기
   - `Block Name` 필드 확인:
     - 비어있으면: GameObject 이름을 자동으로 사용 (예: "NPC_NewMother")
     - 값이 있으면: 해당 값이 InputField에 삽입됨
   - `Click Cooldown`: 중복 클릭 방지 시간 (기본값: 0.1초)

3. **Block Name 변경 (선택사항)**
   - `Block Name` 필드에 원하는 이름 입력
   - 예: "새엄마", "엄마", "NewMother" 등
   - 비워두면 GameObject 이름 사용

---

## 2. 아이템 Prefab 배치

### 2-1. 아이템 Prefab 위치 확인

1. **Project 창 열기**
   - Unity 에디터 하단의 `Project` 탭 클릭

2. **아이템 Prefab 폴더로 이동**
   - Project 창에서 다음 경로로 이동:
     ```
     Assets → Assets → Prefabs → Items
     ```
   - 다음 아이템 Prefab들이 보여야 합니다:
     - `Item_SleepingPill.prefab` (수면제)
     - `Item_EarlGreyTea.prefab` (홍차)
     - `Item_RealFamilyPhoto.prefab` (진짜 가족 사진)
     - `Item_OilBottle.prefab` (기름병)
     - `Item_SilverLighter.prefab` (라이터)
     - `Item_SiblingsToy.prefab` (동생의 장난감)
     - `Item_BrassKey.prefab` (황동 열쇠)

### 2-2. 아이템 Prefab을 씬에 배치

#### 방법 1: 드래그 앤 드롭 (권장)

1. **Project 창에서 아이템 Prefab 선택**
   - 예: `Item_SleepingPill.prefab` 클릭

2. **Hierarchy 창으로 드래그**
   - Project 창의 아이템 Prefab을 마우스로 클릭하고 드래그
   - Hierarchy 창으로 이동
   - Hierarchy 창에서 마우스 버튼 놓기

3. **배치 확인**
   - Hierarchy 창에 아이템 이름이 추가되었는지 확인
   - 예: `Item_SleepingPill` 또는 `Item_SleepingPill (1)`

#### 방법 2: 우클릭 메뉴 사용

1. **Project 창에서 아이템 Prefab 우클릭**
   - `Item_SleepingPill.prefab` 우클릭

2. **"Instantiate" 선택**
   - 우클릭 메뉴에서 `Instantiate` 클릭
   - Hierarchy 창에 아이템이 추가됨

### 2-3. 아이템 위치 조정

1. **Hierarchy에서 아이템 선택**
   - Hierarchy 창에서 방금 추가한 아이템 클릭
   - 예: `Item_SleepingPill`

2. **Scene 뷰 확인**
   - Scene 뷰에서 아이템이 보이는지 확인
   - 아이템이 보이지 않으면 Scene 뷰를 이동/확대

3. **Transform 위치 조정**
   - Inspector 창에서 `Transform` 컴포넌트 확인
   - `Position` 값을 조정:
     - `X`: 좌우 위치
     - `Y`: 상하 위치
     - `Z`: 깊이 (보통 0 유지)
   - 예: `Position (-1, 0.5, 0)` → 아이템을 왼쪽 위로 이동

4. **Scene 뷰에서 직접 이동**
   - Scene 뷰에서 아이템 선택
   - 화살표 모양의 이동 도구 사용 (단축키: `W`)
   - 화살표를 드래그하여 위치 조정

### 2-4. 아이템 Sprite 설정

1. **아이템 선택**
   - Hierarchy에서 아이템 GameObject 선택

2. **SpriteRenderer 컴포넌트 확인**
   - Inspector 창에서 `Sprite Renderer` 컴포넌트 찾기
   - `Sprite` 필드 확인 (현재 `None (Sprite)`로 표시될 수 있음)

3. **Sprite 이미지 준비**
   - `Assets/Assets/Object/` 폴더에 아이템 이미지가 있는지 확인
   - 이미지가 없다면 준비 필요

4. **Sprite 할당**
   - 방법 1: Project 창에서 Sprite 이미지를 찾아 `Sprite` 필드로 드래그 앤 드롭
   - 방법 2: `Sprite` 필드 옆의 원형 아이콘 클릭 → Sprite 이미지 선택

### 2-5. ClickableObject 설정 확인

1. **아이템 선택**
   - Hierarchy에서 아이템 GameObject 선택

2. **ClickableObject 컴포넌트 확인**
   - Inspector 창에서 `Clickable Object` 컴포넌트 찾기
   - `Block Name` 필드 확인:
     - 비어있으면: GameObject 이름을 자동으로 사용 (예: "Item_SleepingPill")
     - 값이 있으면: 해당 값이 InputField에 삽입됨
   - `Click Cooldown`: 중복 클릭 방지 시간 (기본값: 0.1초)

3. **Block Name 변경 (선택사항)**
   - `Block Name` 필드에 원하는 이름 입력
   - 예: "수면제", "SleepingPill" 등
   - 비워두면 GameObject 이름 사용

---

## 3. 씬별 배치 목록

### PlayersRoom (주인공의 방)

**NPC**: 없음

**아이템**: 없음

**상호작용 오브젝트**: 개구멍 (탈출 통로)

---

### Hallway (복도)

**NPC**: 없음 (또는 이동 중 NPC)

**아이템**: 없음

**상호작용 오브젝트**: 없음

---

### LivingRoom (거실)

**NPC**:
- `NPC_NewFather.prefab` (새아빠)

**아이템**: 없음

**상호작용 오브젝트**:
- 벽난로 (`InteractableObject.prefab`)
- 라디오/TV (`InteractableObject.prefab`)

---

### Kitchen (주방)

**NPC**:
- `NPC_NewMother.prefab` (새엄마)

**아이템**:
- `Item_SleepingPill.prefab` (수면제) - 찬장 위치
- `Item_EarlGreyTea.prefab` (홍차) - 식탁 위치

**상호작용 오브젝트**:
- 찬장 (`InteractableObject.prefab`)
- 식탁 (`InteractableObject.prefab`)

---

### SiblingsRoom (동생의 놀이방)

**NPC**:
- `NPC_Sibling.prefab` (동생)

**아이템**:
- `Item_SiblingsToy.prefab` (동생의 장난감)

**상호작용 오브젝트**:
- 인형의 집 모형 (`InteractableObject.prefab`)
- 벽장 (`InteractableObject.prefab`)

---

### Basement (지하실)

**NPC**:
- `NPC_Grandmother.prefab` (할머니)

**아이템**:
- `Item_OilBottle.prefab` (기름병)
- `Item_SilverLighter.prefab` (라이터)

**상호작용 오브젝트**:
- 수술대 (`InteractableObject.prefab`)
- 할머니의 침대 (`InteractableObject.prefab`)

---

### Backyard (뒷마당)

**NPC**:
- `NPC_Dog.prefab` (강아지)

**아이템**:
- `Item_RealFamilyPhoto.prefab` (진짜 가족 사진)

**상호작용 오브젝트**:
- 개집 (`InteractableObject.prefab`)

---

## 4. 문제 해결

### NPC/아이템이 보이지 않을 때

1. **Scene 뷰 확인**
   - Scene 뷰에서 NPC/아이템이 화면 밖에 있는지 확인
   - Scene 뷰를 이동/확대하여 찾기

2. **Transform 위치 확인**
   - Inspector에서 `Transform` → `Position` 확인
   - `Position`을 `(0, 0, 0)`으로 설정하여 화면 중앙에 배치

3. **Sprite 확인**
   - Inspector에서 `Sprite Renderer` → `Sprite` 확인
   - Sprite가 할당되지 않았으면 할당

4. **GameObject 활성화 확인**
   - Hierarchy에서 GameObject 이름 옆의 체크박스 확인
   - 체크박스가 해제되어 있으면 클릭하여 활성화

### Prefab을 찾을 수 없을 때

1. **Project 창에서 경로 확인**
   - `Assets/Assets/Prefabs/NPCs/` 또는 `Assets/Assets/Prefabs/Items/` 경로 확인
   - 폴더가 보이지 않으면 Project 창 상단의 검색창 사용

2. **검색 기능 사용**
   - Project 창 상단의 검색창에 "NPC_" 또는 "Item_" 입력
   - 관련 Prefab이 검색됨

### 드래그 앤 드롭이 작동하지 않을 때

1. **씬이 저장되었는지 확인**
   - 씬 이름 옆에 `*` 표시가 있으면 저장되지 않은 상태
   - `Ctrl + S`로 저장

2. **다른 방법 시도**
   - 우클릭 → `Instantiate` 메뉴 사용
   - 또는 Hierarchy에서 우클릭 → `Create` → `Prefab Instance` 선택

### Sprite 이미지를 찾을 수 없을 때

1. **이미지 파일 위치 확인**
   - NPC: `Assets/Assets/Character/` 폴더
   - 아이템: `Assets/Assets/Object/` 폴더

2. **Import Settings 확인**
   - 이미지 파일 선택 → Inspector에서 `Texture Type` 확인
   - `Sprite (2D and UI)`로 설정되어 있어야 함

3. **이미지 준비**
   - 이미지가 없다면 준비 필요
   - 또는 임시로 다른 Sprite 사용

### ClickableObject가 작동하지 않을 때

1. **컴포넌트 확인**
   - Inspector에서 `Clickable Object` 컴포넌트가 있는지 확인
   - 없다면 `Add Component` → `Clickable Object` 추가

2. **BoxCollider2D 확인**
   - Inspector에서 `Box Collider 2D` 컴포넌트가 있는지 확인
   - 없다면 `Add Component` → `Box Collider 2D` 추가
   - `ClickableObject`는 `BoxCollider2D`가 필요함

3. **크기 확인**
   - `Box Collider 2D`의 `Size` 값 확인
   - 너무 작으면 클릭이 어려울 수 있음
   - `Size` 값을 적절히 조정

---

## 작업 체크리스트

### NPC 배치 체크리스트

- [ ] NPC Prefab을 Hierarchy로 드래그 앤 드롭
- [ ] Transform 위치 조정 (Scene 뷰에서 확인)
- [ ] SpriteRenderer의 Sprite 할당
- [ ] ClickableObject의 Block Name 확인
- [ ] BoxCollider2D 크기 확인 (필요시 조정)

### 아이템 배치 체크리스트

- [ ] 아이템 Prefab을 Hierarchy로 드래그 앤 드롭
- [ ] Transform 위치 조정 (Scene 뷰에서 확인)
- [ ] SpriteRenderer의 Sprite 할당
- [ ] ClickableObject의 Block Name 확인
- [ ] BoxCollider2D 크기 확인 (필요시 조정)

---

## 팁

### 빠른 배치 팁

1. **여러 개 배치**
   - 같은 NPC/아이템을 여러 개 배치할 때:
   - 첫 번째 배치 후 `Ctrl + D` (Duplicate)로 복제
   - 위치만 조정

2. **정렬**
   - 여러 NPC/아이템을 배치할 때:
   - 첫 번째 선택 → `Ctrl + C` (복사)
   - 두 번째 위치에서 `Ctrl + V` (붙여넣기)
   - Transform의 Position 값만 조정

3. **이름 변경**
   - Hierarchy에서 GameObject 선택
   - `F2` 키를 누르거나 더블클릭하여 이름 변경
   - 예: `NPC_NewMother (1)` → `NPC_NewMother`

### Scene 뷰 사용 팁

1. **이동**: 마우스 휠 클릭 + 드래그
2. **확대/축소**: 마우스 휠 스크롤
3. **회전**: `Alt` + 마우스 왼쪽 버튼 드래그
4. **2D 모드**: Scene 뷰 상단의 "2D" 버튼 클릭

---

**작성일**: 2025-02-11  
**최종 수정일**: 2025-02-11

