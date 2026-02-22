using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 엔딩을 트리거하는 매니저입니다.
/// 엔딩 조건 판단은 백엔드에서 처리하며, 백엔드 응답의 ending_info를 통해 엔딩을 트리거합니다.
/// </summary>
public class EndingManager
{
    private EndingType currentEnding = EndingType.None;
    
    private string gameOverSceneName = "GameOver";
    private float gameOverFadeDuration = 1f;

    /// <summary>
    /// 엔딩 트리거 시 호출되는 이벤트입니다.
    /// </summary>
    public event System.Action<EndingType> OnEndingTriggered;

    /// <summary>
    /// 초기화합니다.
    /// </summary>
    /// <param name="humanityMgr">HumanityManager (사용하지 않음, 호환성 유지)</param>
    /// <param name="dayMgr">DayManager (사용하지 않음, 호환성 유지)</param>
    /// <param name="inventoryMgr">InventoryManager (사용하지 않음, 호환성 유지)</param>
    /// <param name="npcMgr">NPCManager (사용하지 않음, 호환성 유지)</param>
    /// <param name="locationMgr">LocationManager (사용하지 않음, 호환성 유지)</param>
    /// <param name="turnMgr">TurnManager (사용하지 않음, 호환성 유지)</param>
    /// <param name="eventFlagMgr">EventFlagManager (사용하지 않음, 호환성 유지)</param>
    public void Initialize(
        HumanityManager humanityMgr,
        DayManager dayMgr,
        InventoryManager inventoryMgr,
        NPCManager npcMgr,
        LocationManager locationMgr,
        TurnManager turnMgr,
        EventFlagManager eventFlagMgr)
    {
        // 엔딩 조건 판단은 백엔드에서 처리하므로 매니저 참조가 필요 없음
        // 호환성을 위해 파라미터는 유지하되 사용하지 않음
    }

    /// <summary>
    /// 엔딩을 트리거합니다. 내러티브 표시 후 사용자 클릭 시 LoadEndingScene()을 호출해야 씬으로 전환됩니다.
    /// </summary>
    public void TriggerEnding(EndingType ending)
    {
        if (ending == EndingType.None)
        {
            Debug.LogWarning("[EndingManager] 유효하지 않은 엔딩 타입입니다.");
            return;
        }
        
        currentEnding = ending;
        OnEndingTriggered?.Invoke(ending);
        
        Debug.Log($"[EndingManager] 엔딩 트리거 (클릭 시 씬 전환): {ending}");
        
        // GameStateManager에 현재 엔딩 타입 저장
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.SetCurrentEnding(ending);
        }
        
        // 씬 로드는 사용자 클릭 후 LoadEndingScene() 호출 시 수행
    }

    /// <summary>
    /// 엔딩이 트리거되었으나 아직 씬 전환이 이루어지지 않은 대기 상태인지 여부를 반환합니다.
    /// </summary>
    public bool HasPendingEnding()
    {
        return currentEnding != EndingType.None;
    }

    /// <summary>
    /// 엔딩 씬(GameOver)으로 전환합니다. HasPendingEnding()이 true일 때만 호출해야 합니다.
    /// </summary>
    public void LoadEndingScene()
    {
        if (currentEnding == EndingType.None)
        {
            Debug.LogWarning("[EndingManager] 전환할 엔딩이 없습니다.");
            return;
        }

        string endingSceneName = GetEndingSceneName(currentEnding);
        if (!string.IsNullOrEmpty(endingSceneName))
        {
            SceneFadeManager fadeManager = Object.FindFirstObjectByType<SceneFadeManager>();
            if (fadeManager != null)
            {
                fadeManager.LoadSceneWithFade(endingSceneName, gameOverFadeDuration);
            }
            else
            {
                SceneManager.LoadScene(endingSceneName);
            }
        }
    }

    /// <summary>
    /// 엔딩 타입에 따른 씬 이름을 반환합니다.
    /// 모든 엔딩이 GameOver 씬으로 통합되었습니다.
    /// </summary>
    private string GetEndingSceneName(EndingType ending)
    {
        // 모든 엔딩이 GameOver 씬으로 통합됨
        return "GameOver";
    }

    /// <summary>
    /// 게임 오버 설정을 설정합니다.
    /// </summary>
    public void SetGameOverSettings(string sceneName, float fadeDuration)
    {
        gameOverSceneName = sceneName;
        gameOverFadeDuration = fadeDuration;
    }
}

