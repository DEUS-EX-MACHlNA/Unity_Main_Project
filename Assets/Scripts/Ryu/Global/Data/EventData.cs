using System;
using System.Collections.Generic;

/// <summary>
/// 이벤트 플래그
/// </summary>
[Serializable]
public class EventFlags
{
    public bool grandmotherCooperation;    // 할머니 협력 상태
    public bool holeUnlocked;             // 개구멍 개방 상태
    public bool fireStarted;              // 화재 발생 상태
    public bool familyAsleep;             // 가족 수면 상태
    public bool teaWithSleepingPill;      // 홍차에 수면제를 탔는지 여부 (StealthExit 엔딩 조건)
    public bool keyStolen;                // 열쇠 탈취 상태
    // Unity JsonUtility는 Dictionary를 지원하지 않으므로, 직렬화가 필요한 경우 별도 처리 필요
    public Dictionary<string, bool> customEvents; // 기타 커스텀 이벤트
}

