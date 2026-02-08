using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 게임 전반의 상태를 중앙에서 관리하는 싱글톤 매니저입니다.
/// 인간성 수치를 관리하고 게임 상태를 추적합니다.
/// </summary>
public class GameStateManager : MonoBehaviour
{
    // Singleton 인스턴스
    public static GameStateManager Instance { get; private set; }

    // 상태 변수
    private float humanity = 100f; // 초기값 100%
    private const float MIN_HUMANITY = 0f;
    private const float MAX_HUMANITY = 100f;

    // 날짜 변수
    private int currentDay = 1; // 1일차부터 시작 (기본값 1)
    private const int MAX_DAY = 5; // 최대 5일차

    /// <summary>
    /// 현재 날짜를 반환합니다 (1~5일차).
    /// </summary>
    public int CurrentDay 
    { 
        get { return currentDay; } 
        private set { currentDay = Mathf.Clamp(value, 1, MAX_DAY); }
    }

    // 이벤트 시스템
    /// <summary>
    /// 인간성 변경 시 호출되는 이벤트입니다.
    /// </summary>
    public event Action<float> OnHumanityChanged;

    /// <summary>
    /// 인간성 0% 도달 시 호출되는 이벤트입니다.
    /// </summary>
    public event Action OnHumanityReachedZero;

    /// <summary>
    /// 날짜가 변경될 때 호출되는 이벤트입니다. (새로운 날짜)
    /// </summary>
    public event Action<int> OnDayChanged;

    /// <summary>
    /// 싱글톤 패턴 초기화 및 게임 상태 초기화를 수행합니다.
    /// </summary>
    private void Awake()
    {
        // Singleton 패턴
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 초기값 설정
        humanity = 10f; // 테스트용: 10%로 설정
        currentDay = 1; // 1일차부터 시작
    }

    /// <summary>
    /// 현재 인간성 수치를 반환합니다 (0~100).
    /// </summary>
    /// <returns>현재 인간성 수치 (0~100)</returns>
    public float GetHumanity()
    {
        return humanity;
    }

    /// <summary>
    /// 현재 날짜를 반환합니다.
    /// </summary>
    public int GetCurrentDay()
    {
        return CurrentDay;
    }

    /// <summary>
    /// 다음 날로 진행합니다. 시간 흐름에 따른 인간성 감소가 적용됩니다.
    /// 밤을 지나면 자동으로 다음 날로 넘어갑니다.
    /// </summary>
    /// <returns>게임 오버가 발생했으면 true, 정상 진행이면 false</returns>
    public bool AdvanceToNextDay()
    {
        if (currentDay < MAX_DAY)
        {
            currentDay++;
            CurrentDay = currentDay; // CurrentDay 프로퍼티를 통해 값 설정 (클램프 포함)
            
            // 인간성 10% 감소 (시간 경과 페널티)
            float oldHumanity = humanity;
            ModifyHumanity(-10f);
            
            // 게임 오버가 발생했는지 확인 (ModifyHumanity 내부에서 TriggerGameOver가 호출됨)
            bool gameOverOccurred = humanity <= MIN_HUMANITY && oldHumanity > MIN_HUMANITY;
            
            if (!gameOverOccurred)
            {
                OnDayChanged?.Invoke(CurrentDay);
                Debug.Log($"[GameStateManager] 다음 날로 진행: {CurrentDay}일차 (최대 {MAX_DAY}일차)");
            }
            
            return gameOverOccurred;
        }
        else
        {
            Debug.LogWarning($"[GameStateManager] 최대 일수({MAX_DAY}일)에 도달했습니다. 게임 종료 조건 확인 필요.");
            // TODO: 5일차 종료 시 게임 오버/엔딩 조건 처리
            return false;
        }
    }

    /// <summary>
    /// 다음 날로 진행합니다. AdvanceToNextDay()의 별칭 메서드입니다.
    /// </summary>
    /// <returns>게임 오버가 발생했으면 true, 정상 진행이면 false</returns>
    public bool AdvanceDay()
    {
        return AdvanceToNextDay();
    }

    /// <summary>
    /// 인간성 수치를 변경합니다.
    /// 백엔드에서 받은 변화량을 적용합니다.
    /// </summary>
    /// <param name="changeAmount">변화량 (양수: 증가, 음수: 감소)</param>
    public void ModifyHumanity(float changeAmount)
    {
        float oldValue = humanity;
        humanity = Mathf.Clamp(humanity + changeAmount, MIN_HUMANITY, MAX_HUMANITY);

        // 이벤트 발생
        OnHumanityChanged?.Invoke(humanity);

        // 게임 오버 체크
        if (humanity <= MIN_HUMANITY && oldValue > MIN_HUMANITY)
        {
            OnHumanityReachedZero?.Invoke();
            TriggerGameOver();
        }

        Debug.Log($"[GameStateManager] 인간성 변경: {oldValue:F1}% → {humanity:F1}% (변화량: {changeAmount:F1})");
    }

    /// <summary>
    /// 테스트용: 인간성 수치를 직접 설정합니다.
    /// </summary>
    /// <param name="value">설정할 인간성 수치 (0~100)</param>
    public void SetHumanity(float value)
    {
        float oldValue = humanity;
        humanity = Mathf.Clamp(value, MIN_HUMANITY, MAX_HUMANITY);
        
        // 이벤트 발생
        OnHumanityChanged?.Invoke(humanity);
        
        // 게임 오버 체크
        if (humanity <= MIN_HUMANITY && oldValue > MIN_HUMANITY)
        {
            OnHumanityReachedZero?.Invoke();
            TriggerGameOver();
        }
        
        Debug.Log($"[GameStateManager] 테스트: 인간성 수치 설정 {oldValue:F1}% → {humanity:F1}%");
    }

    [Header("Game Over Settings")]
    [SerializeField] private string gameOverSceneName = "GameOver";
    [SerializeField] private float gameOverFadeDuration = 1f;

    /// <summary>
    /// 인간성 0% 도달 시 배드 엔딩을 트리거합니다.
    /// </summary>
    private void TriggerGameOver()
    {
        Debug.LogWarning("[GameStateManager] 인간성이 0%에 도달했습니다. 게임 오버!");
        
        // SceneFadeManager를 찾아서 GameOver 씬으로 전환
        SceneFadeManager fadeManager = FindObjectOfType<SceneFadeManager>();
        if (fadeManager != null)
        {
            fadeManager.LoadSceneWithFade(gameOverSceneName, gameOverFadeDuration);
        }
        else
        {
            Debug.LogWarning("[GameStateManager] SceneFadeManager를 찾을 수 없습니다. 페이드 없이 씬을 전환합니다.");
            SceneManager.LoadScene(gameOverSceneName);
        }
    }

    /// <summary>
    /// 최대 일수를 반환합니다.
    /// </summary>
    public int GetMaxDay()
    {
        return MAX_DAY;
    }

    /// <summary>
    /// 싱글톤 인스턴스를 완전히 초기화합니다. 게임 재시작 시 사용됩니다.
    /// </summary>
    public static void ClearInstance()
    {
        if (Instance != null)
        {
            GameObject oldInstance = Instance.gameObject;
            Destroy(oldInstance);
            
            // 리플렉션을 사용하여 private setter로 Instance를 null로 설정
            PropertyInfo propertyInfo = typeof(GameStateManager).GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
            if (propertyInfo != null)
            {
                propertyInfo.SetValue(null, null);
            }
            
            Debug.Log("[GameStateManager] Instance가 초기화되었습니다.");
        }
    }
}

