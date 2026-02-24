# 프로젝트 인형의 집 (Project: Puppet Home)

> "가족의 가면을 쓴 인형들 사이에서 5일 안에 인간성을 유지하며 탈출해야 하는 심리 스릴러"

## 📋 프로젝트 개요

**프로젝트 인형의 집**은 AI 기반 리얼타임 자유 서술 스릴러이자 비주얼 추론 어드벤처 게임입니다. 플레이어는 기괴한 인형의 세계에서 깨어나, 서서히 사라져가는 자신의 '인간성'을 붙잡고 가족들의 감시를 피해 탈출 방법을 찾아야 합니다.

### 주요 특징

- 🤖 **AI 기반 자유 서술 시스템**: LLM을 활용한 자연어 처리로 플레이어의 의도와 행동을 분석
- 🎯 **포인트 앤 클릭 탐색**: 슬라이드 뷰 방식의 방 탐색 및 상호작용
- ⚖️ **인간성 시스템**: 시간이 지날수록 감소하는 인간성 수치로 게임 오버 조건 결정
- 📦 **인벤토리 및 아이템 관리**: 다양한 아이템을 수집하고 활용하여 탈출 루트 결정
- 👥 **NPC 관계 시스템**: 호감도와 인간성에 따라 변화하는 NPC 반응
- 🌙 **밤의 대화 시스템**: 플레이어가 잠든 사이 NPC들 간의 회의 시뮬레이션
- 🎬 **다중 엔딩**: 플레이어의 선택과 행동에 따라 달라지는 5가지 엔딩

## 🛠️ 기술 스택

### 게임 엔진 및 프레임워크
- **Unity**: 2022.3 LTS 이상
- **Universal Render Pipeline (URP)**: 2D 렌더링 파이프라인
- **TextMesh Pro**: 고품질 텍스트 렌더링
- **Unity Input System**: 입력 처리

### 주요 패키지
- `com.unity.inputsystem`: 1.17.0
- `com.unity.render-pipelines.universal`: 17.3.0
- `com.unity.nuget.newtonsoft-json`: 3.2.2
- `com.unity.2d.animation`: 13.0.2
- `com.unity.2d.aseprite`: 3.0.1

### 백엔드 연동
- RESTful API 통신
- JSON 기반 데이터 교환
- LLM 기반 게임 스텝 처리

## 📁 프로젝트 구조

```
Unity_Main_Project/
├── Assets/
│   ├── Scripts/
│   │   ├── Ryu/                    # 메인 게임 로직
│   │   │   ├── Global/             # 전역 시스템
│   │   │   │   ├── API/            # 백엔드 API 클라이언트
│   │   │   │   ├── Data/           # 데이터 타입 정의
│   │   │   │   ├── Enums/          # 열거형 정의
│   │   │   │   ├── Managers/       # 게임 매니저들
│   │   │   │   ├── State/          # 상태 적용 로직
│   │   │   │   └── Utils/          # 유틸리티
│   │   │   ├── Gameplay/           # 게임플레이 로직
│   │   │   ├── Title/              # 타이틀 화면
│   │   │   ├── Night/              # 밤 대화 시스템
│   │   │   └── UI/                 # UI 관련
│   │   ├── Bi/                     # 레거시 코드
│   │   └── Editor/                 # 에디터 확장
│   ├── Scenes/                     # 게임 씬들
│   │   ├── Ryu/                    # 메인 게임 씬
│   │   └── Bi/                     # 레거시 씬
│   ├── Audio/                      # 오디오 리소스
│   ├── sprites/                    # 스프라이트 리소스
│   ├── TestData/                   # 테스트 데이터
│   └── Settings/                   # 프로젝트 설정
├── 마크다운/                       # 문서화
│   ├── 시나리오/                   # 게임 시나리오
│   ├── 변경사항/                   # 개발 기록
│   └── 백엔드/                     # 백엔드 스펙
└── ProjectSettings/                # Unity 프로젝트 설정
```

## 🎮 주요 시스템

### 1. 게임 상태 관리 (GameStateManager)

게임 전반의 상태를 중앙에서 관리하는 싱글톤 매니저입니다. 각 시스템 매니저들을 조율하는 오케스트레이터 역할을 합니다.

**주요 매니저들:**
- `HumanityManager`: 인간성 수치 관리
- `DayManager`: 날짜 및 시간대 관리
- `NPCManager`: NPC 상태 및 관계 관리
- `InventoryManager`: 인벤토리 관리
- `ItemStateManager`: 아이템 상태 관리
- `TurnManager`: 턴 시스템 관리
- `LocationManager`: 위치 관리
- `EventFlagManager`: 이벤트 플래그 관리
- `EndingManager`: 엔딩 조건 관리

### 2. API 통신 시스템

백엔드 서버와의 통신을 담당하는 시스템입니다.

**주요 클라이언트:**
- `ApiClient`: 기본 API 통신
- `GameStepApiClient`: 게임 스텝 처리
- `NightDialogueApiClient`: 밤 대화 처리
- `ScenarioStartApiClient`: 시나리오 시작
- `BackendResponseConverter`: 백엔드 응답 변환

### 3. 상태 적용 시스템 (State Appliers)

백엔드에서 받은 상태 변경을 게임에 적용하는 시스템입니다.

- `GameStateApplier`: 게임 상태 적용
- `ItemStateApplier`: 아이템 상태 적용
- `NPCStateApplier`: NPC 상태 적용
- `EventFlagApplier`: 이벤트 플래그 적용

### 4. 씬 전환 시스템

- `SceneTransitionArea`: 씬 전환 영역 클릭 감지
- `SceneFadeManager`: 페이드 효과 관리
- `DoorTooltipManager`: 문 호버 툴팁 표시

### 5. 오디오 시스템

- `AudioManager`: 배경음악 및 효과음 관리
- 페이드 인/아웃 효과 지원

## 🚀 시작하기

### 필수 요구사항

- Unity 2022.3 LTS 이상
- Git (프로젝트 클론용)
- 백엔드 API 서버 (게임 실행을 위해 필요)

### 설치 방법

1. **프로젝트 클론**
   ```bash
   git clone <repository-url>
   cd Unity_Main_Project
   ```

2. **Unity에서 프로젝트 열기**
   - Unity Hub를 실행
   - "Open" 버튼 클릭
   - `Unity_Main_Project` 폴더 선택

3. **의존성 설치**
   - Unity가 자동으로 Package Manager를 통해 필요한 패키지를 다운로드합니다
   - 완료까지 몇 분이 소요될 수 있습니다

4. **프로젝트 설정 확인**
   - `ProjectSettings/ProjectSettings.asset`에서 프로젝트 설정 확인
   - API 엔드포인트 설정 확인 (필요 시)

### 실행 방법

1. Unity 에디터에서 `Assets/Scenes/Ryu/Title/Title.unity` 씬 열기
2. Play 버튼 클릭하여 게임 실행
3. 타이틀 화면에서 "시작" 버튼 클릭

## 🎯 게임 플레이

### 기본 조작

- **마우스 클릭**: 상호작용 가능한 오브젝트 선택
- **텍스트 입력**: 자유 서술로 행동 및 대화 입력
- **씬 전환**: 문이나 전환 영역 클릭

### 게임 루프

1. **낮 (탐색 및 대화)**
   - 행동력(턴)을 소모하여 조사, 아이템 습득, NPC와 대화
   - 일일 10턴 제한

2. **밤의 대화**
   - 플레이어가 잠든 사이 NPC들끼리 회의 진행
   - 플레이어와의 상호작용 경험 공유
   - 호감도 재계산

3. **다음 날**
   - 날짜 증가
   - 인간성 자동 감소
   - 새로운 이벤트 발생 가능

### 주요 목표

- **인간성 유지**: 인간성이 0%가 되면 게임 오버
- **아이템 수집**: 탈출에 필요한 핵심 아이템 획득
- **NPC 관계 관리**: 호감도와 인간성을 조절하여 조력자 확보
- **5일 안에 탈출**: 시간 제한 내에 탈출 방법 찾기

## 📚 주요 문서

프로젝트의 상세 문서는 `마크다운/` 폴더에 있습니다:

- **시나리오**: `마크다운/시나리오/시나리오.md` - 게임 기획안 및 시나리오
- **변경사항**: `마크다운/변경사항/` - 개발 기록 및 구현 계획
- **백엔드**: `마크다운/백엔드/` - 백엔드 API 스펙

## 🏗️ 개발 가이드

### 코드 구조

- **싱글톤 패턴**: `GameStateManager`, `ApiClient` 등 주요 매니저는 싱글톤으로 구현
- **이벤트 시스템**: 상태 변경 시 이벤트를 통해 UI 및 다른 시스템에 알림
- **매니저 패턴**: 각 시스템별로 전용 매니저 클래스 분리

### 네이밍 컨벤션

- **클래스**: PascalCase (예: `GameStateManager`)
- **메서드**: PascalCase (예: `AddItem`)
- **변수**: camelCase (예: `currentDay`)
- **상수**: UPPER_SNAKE_CASE (예: `PLAYERS_ROOM_SCENE_NAME`)

### 주요 열거형

- `NPCType`: NPC 타입 (NewMother, NewFather, Sibling, Dog, Grandmother)
- `ItemType`: 아이템 타입 (SleepingPills, EarlGreyTea, BrassKey 등)
- `ItemState`: 아이템 상태 (InWorld, InInventory, Used, Hidden)
- `EndingType`: 엔딩 타입 (StealthExit, ChaoticBreakout, SiblingsHelp 등)
- `GameLocation`: 게임 위치 (PlayersRoom, Hallway, LivingRoom 등)

## 🐛 알려진 이슈

- 일부 씬에서 씬 전환 시 효과음 재생 문제 (해결 중)
- 인벤토리 UI 갱신 타이밍 이슈 (개선 예정)

## 📝 라이선스

이 프로젝트는 비공개 프로젝트입니다.

## 👥 기여자

프로젝트 개발팀

## 📞 문의

프로젝트 관련 문의사항이 있으시면 이슈를 등록해주세요.

---

**마지막 업데이트**: 2026-02-23

