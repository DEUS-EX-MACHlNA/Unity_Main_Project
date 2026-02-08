using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum NpcId
{
    stepmother,
    stepfather,
    brother,
    dog,
    grandmother
}

[System.Serializable]
public class NpcStateData
{
    public NpcId id;
    public int affection;  // 0~100
    public int humanity;   // -100~100

    public NpcStateData(NpcId npcId, int initialAffection = 50, int initialHumanity = 0)
    {
        id = npcId;
        affection = Mathf.Clamp(initialAffection, 0, 100);
        humanity = Mathf.Clamp(initialHumanity, -100, 100);
    }
}

public class NpcStateManager : MonoBehaviour
{
    [Header("NPC 상태 초기값")]
    [SerializeField] private List<NpcStateData> npcStates = new List<NpcStateData>();

    [Header("플레이어 상태")]
    [SerializeField] [Range(0, 100)] private int playerHumanity = 100;
    [SerializeField] private List<string> playerFlags = new List<string>();

    [Header("게임 컨텍스트")]
    [SerializeField] private string currentLocation = "night_room";
    [SerializeField] private List<string> recentPlayerActions = new List<string>();

    private static NpcStateManager _instance;
    public static NpcStateManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<NpcStateManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("NpcStateManager");
                    _instance = go.AddComponent<NpcStateManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeNpcStates();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (npcStates.Count == 0)
        {
            InitializeNpcStates();
        }
    }

    /// <summary>
    /// NPC 상태를 초기화합니다.
    /// </summary>
    private void InitializeNpcStates()
    {
        if (npcStates.Count == 0)
        {
            npcStates.Add(new NpcStateData(NpcId.stepmother, 30, -100));
            npcStates.Add(new NpcStateData(NpcId.stepfather, 45, 10));
            npcStates.Add(new NpcStateData(NpcId.brother, 70, 40));
            npcStates.Add(new NpcStateData(NpcId.dog, 60, 100));
            npcStates.Add(new NpcStateData(NpcId.grandmother, 0, 0));
        }
    }

    /// <summary>
    /// NPC 상태를 조회합니다.
    /// </summary>
    public NpcStateData GetNpcState(NpcId npcId)
    {
        var state = npcStates.FirstOrDefault(n => n.id == npcId);
        if (state == null)
        {
            state = new NpcStateData(npcId);
            npcStates.Add(state);
        }
        return state;
    }

    /// <summary>
    /// NPC 상태를 조회합니다 (문자열 ID로).
    /// </summary>
    public NpcStateData GetNpcState(string npcIdString)
    {
        if (System.Enum.TryParse<NpcId>(npcIdString, true, out NpcId npcId))
        {
            return GetNpcState(npcId);
        }
        Debug.LogWarning($"[NpcStateManager] 알 수 없는 NPC ID: {npcIdString}");
        return null;
    }

    /// <summary>
    /// NPC 상태를 업데이트합니다.
    /// </summary>
    public void UpdateNpcState(NpcId npcId, int? affectionDelta = null, int? humanityDelta = null)
    {
        var state = GetNpcState(npcId);
        if (affectionDelta.HasValue)
        {
            state.affection = Mathf.Clamp(state.affection + affectionDelta.Value, 0, 100);
        }
        if (humanityDelta.HasValue)
        {
            state.humanity = Mathf.Clamp(state.humanity + humanityDelta.Value, -100, 100);
        }
    }

    /// <summary>
    /// NPC 상태를 업데이트합니다 (문자열 ID로).
    /// </summary>
    public void UpdateNpcState(string npcIdString, int? affectionDelta = null, int? humanityDelta = null)
    {
        if (System.Enum.TryParse<NpcId>(npcIdString, true, out NpcId npcId))
        {
            UpdateNpcState(npcId, affectionDelta, humanityDelta);
        }
    }

    /// <summary>
    /// 플레이어 인간성을 조회합니다.
    /// </summary>
    public int GetPlayerHumanity()
    {
        return playerHumanity;
    }

    /// <summary>
    /// 플레이어 인간성을 설정합니다.
    /// </summary>
    public void SetPlayerHumanity(int value)
    {
        playerHumanity = Mathf.Clamp(value, 0, 100);
    }

    /// <summary>
    /// 플레이어 인간성을 변경합니다.
    /// </summary>
    public void UpdatePlayerHumanity(int delta)
    {
        playerHumanity = Mathf.Clamp(playerHumanity + delta, 0, 100);
    }

    /// <summary>
    /// 플레이어 플래그를 추가합니다.
    /// </summary>
    public void AddPlayerFlag(string flag)
    {
        if (!playerFlags.Contains(flag))
        {
            playerFlags.Add(flag);
        }
    }

    /// <summary>
    /// 플레이어 플래그를 제거합니다.
    /// </summary>
    public void RemovePlayerFlag(string flag)
    {
        playerFlags.Remove(flag);
    }

    /// <summary>
    /// 플레이어 플래그를 확인합니다.
    /// </summary>
    public bool HasPlayerFlag(string flag)
    {
        return playerFlags.Contains(flag);
    }

    /// <summary>
    /// 최근 플레이어 행동을 추가합니다.
    /// </summary>
    public void AddRecentAction(string action)
    {
        recentPlayerActions.Add(action);
        // 최근 10개만 유지
        if (recentPlayerActions.Count > 10)
        {
            recentPlayerActions.RemoveAt(0);
        }
    }

    /// <summary>
    /// 현재 위치를 설정합니다.
    /// </summary>
    public void SetLocation(string location)
    {
        currentLocation = location;
    }

    /// <summary>
    /// 밤의 대화 API 요청용 데이터를 생성합니다.
    /// </summary>
    public NightRequest GetNightRequestData(int day, int turnSpentToday)
    {
        // NPC 상태 배열 생성
        NpcState[] npcs = new NpcState[npcStates.Count];
        for (int i = 0; i < npcStates.Count; i++)
        {
            npcs[i] = new NpcState
            {
                id = npcStates[i].id.ToString(),
                affection = npcStates[i].affection,
                humanity = npcStates[i].humanity
            };
        }

        // 플레이어 상태 생성
        PlayerState player = new PlayerState
        {
            humanity = playerHumanity,
            flags = playerFlags.ToArray()
        };

        // 게임 컨텍스트 생성
        GameContext context = new GameContext
        {
            location = currentLocation,
            recentPlayerActions = recentPlayerActions.ToArray()
        };

        // 요청 데이터 생성
        NightRequest request = new NightRequest
        {
            day = day,
            turnSpentToday = turnSpentToday,
            player = player,
            npcs = npcs,
            context = context
        };

        return request;
    }
}

