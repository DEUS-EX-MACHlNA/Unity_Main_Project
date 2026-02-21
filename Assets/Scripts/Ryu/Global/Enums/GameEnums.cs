/// <summary>
/// 게임에서 사용되는 모든 열거형 정의
/// </summary>

/// <summary>
/// NPC 타입 열거형
/// </summary>
public enum NPCType
{
    NewMother,      // 새엄마 (엘리노어) - 최종보스, 인간성 불가
    NewFather,      // 새아빠 (아더)
    Sibling,        // 동생 (루카스)
    Dog,            // 강아지 (바론)
    Grandmother     // 할머니 (마가렛)
}

/// <summary>
/// 아이템 타입 열거형
/// </summary>
public enum ItemType
{
    None = 0,           // 없음 (기본값)
    SleepingPills,      // 보라색 라벨의 약병 (수면제) - sleeping_pill (문서 기준)
    EarlGreyTea,       // 홍차 - earl_grey_tea
    RealFamilyPhoto,   // 훼손된 가족 사진 (진짜 가족 사진) - real_family_photo
    WhaleOilCan,       // 고래기름 통 (기름병) - oil_bottle (문서 기준)
    SilverLighter,     // 은색 지포 라이터 - lighter
    BrassKey           // 황동 열쇠 (마스터 키) - brass_key
}

/// <summary>
/// 아이템 상태 열거형
/// </summary>
public enum ItemState
{
    InWorld,        // 월드에 존재 (획득 가능)
    InInventory,    // 인벤토리에 있음
    Used,           // 사용됨 (소모품)
    Hidden          // 숨김
}

/// <summary>
/// 시간대 열거형
/// </summary>
public enum TimeOfDay
{
    Day,            // 낮 (탐색 및 대화 가능)
    Night           // 밤 (NPC 회의 진행)
}

/// <summary>
/// 게임 위치 열거형
/// </summary>
public enum GameLocation
{
    PlayersRoom,        // 주인공의 방 (The Birdcage)
    Hallway,            // 복도와 계단 (The Veins)
    LivingRoom,         // 거실 (The Showroom)
    Kitchen,            // 주방 및 식당 (The Modification Lab)
    SiblingsRoom,       // 동생의 놀이방 (The Glitch Room)
    Basement,           // 지하실 (The Archive / 할머니의 방)
    Backyard            // 뒷마당과 정원 (The Gateway)
}

/// <summary>
/// 엔딩 타입 열거형
/// </summary>
public enum EndingType
{
    None,                   // 엔딩 없음
    StealthExit,            // 완벽한 기만 (루트 1)
    ChaoticBreakout,        // 혼돈의 밤 (루트 2)
    SiblingsHelp,           // 조력자의 희생 (루트 4)
    UnfinishedDoll,         // 불완전한 박제 (배드 루트 1)
    EternalDinner           // 영원한 식사 시간 (배드 루트 2)
}

