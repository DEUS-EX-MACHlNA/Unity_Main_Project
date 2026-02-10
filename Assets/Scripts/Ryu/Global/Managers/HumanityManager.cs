using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 플레이어의 인간성 수치를 관리하는 매니저입니다.
/// </summary>
public class HumanityManager
{
    private float humanity = 100f;
    private const float MIN_HUMANITY = 0f;
    private const float MAX_HUMANITY = 100f;
    
    private string gameOverSceneName = "GameOver";
    private float gameOverFadeDuration = 1f;

    /// <summary>
    /// 인간성 변경 시 호출되는 이벤트입니다.
    /// </summary>
    public event System.Action<float> OnHumanityChanged;

    /// <summary>
    /// 인간성 0% 도달 시 호출되는 이벤트입니다.
    /// </summary>
    public event System.Action OnHumanityReachedZero;

    /// <summary>
    /// 현재 인간성 수치를 반환합니다 (0~100).
    /// </summary>
    public float GetHumanity()
    {
        return humanity;
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

        Debug.Log($"[HumanityManager] 인간성 변경: {oldValue:F1}% → {humanity:F1}% (변화량: {changeAmount:F1})");
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
        
        Debug.Log($"[HumanityManager] 테스트: 인간성 수치 설정 {oldValue:F1}% → {humanity:F1}%");
    }

    /// <summary>
    /// 인간성 0% 도달 시 배드 엔딩을 트리거합니다.
    /// </summary>
    private void TriggerGameOver()
    {
        Debug.LogWarning("[HumanityManager] 인간성이 0%에 도달했습니다. 게임 오버!");
        
        // SceneFadeManager를 찾아서 GameOver 씬으로 전환
        SceneFadeManager fadeManager = Object.FindFirstObjectByType<SceneFadeManager>();
        if (fadeManager != null)
        {
            fadeManager.LoadSceneWithFade(gameOverSceneName, gameOverFadeDuration);
        }
        else
        {
            Debug.LogWarning("[HumanityManager] SceneFadeManager를 찾을 수 없습니다. 페이드 없이 씬을 전환합니다.");
            SceneManager.LoadScene(gameOverSceneName);
        }
    }

    /// <summary>
    /// 초기값을 설정합니다.
    /// </summary>
    public void Initialize(float initialHumanity = 10f)
    {
        humanity = initialHumanity;
    }

    /// <summary>
    /// 게임 오버 설정을 설정합니다.
    /// </summary>
    public void SetGameOverSettings(string sceneName, float fadeDuration)
    {
        gameOverSceneName = sceneName;
        gameOverFadeDuration = fadeDuration;
    }

    /// <summary>
    /// 인간성이 0 이하인지 확인합니다.
    /// </summary>
    public bool IsHumanityZero()
    {
        return humanity <= MIN_HUMANITY;
    }
}

