using UnityEngine;
using TMPro;

/// <summary>
/// 인간성 수치를 텍스트로 표시하는 UI 컴포넌트입니다.
/// </summary>
public class HumanityText : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI humanityText;
    
    [Header("Visual Settings")]
    [SerializeField] private Color highHumanityColor = new Color(0.2f, 0.8f, 0.2f); // 밝은 녹색
    [SerializeField] private Color midHumanityColor = new Color(0.8f, 0.8f, 0.2f);   // 노란색
    [SerializeField] private Color lowHumanityColor = new Color(0.8f, 0.2f, 0.2f); // 빨간색
    [SerializeField] private float warningThreshold = 20f; // 경고 표시 임계값
    
<<<<<<< Updated upstream
    private void OnEnable()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnHumanityChanged += UpdateHumanityDisplay;
=======
    private bool isEventSubscribed = false;
    
    private void OnEnable()
    {
        TrySubscribeToGameStateManager();
    }
    
    private void Start()
    {
        // Start에서도 한 번 더 시도 (GameStateManager가 나중에 초기화될 수 있음)
        if (!isEventSubscribed)
        {
            TrySubscribeToGameStateManager();
        }
    }
    
    /// <summary>
    /// GameStateManager에 이벤트를 구독합니다.
    /// </summary>
    private void TrySubscribeToGameStateManager()
    {
        if (isEventSubscribed)
            return;
            
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnHumanityChanged += UpdateHumanityDisplay;
            isEventSubscribed = true;
>>>>>>> Stashed changes
            // 초기값 표시
            UpdateHumanityDisplay(GameStateManager.Instance.GetHumanity());
        }
        else
        {
<<<<<<< Updated upstream
            Debug.LogWarning("[HumanityText] GameStateManager.Instance를 찾을 수 없습니다.");
=======
            // GameStateManager가 아직 초기화되지 않았을 수 있으므로
            // 코루틴으로 지연 체크
            StartCoroutine(WaitForGameStateManager());
        }
    }
    
    /// <summary>
    /// GameStateManager가 초기화될 때까지 대기한 후 구독합니다.
    /// </summary>
    private System.Collections.IEnumerator WaitForGameStateManager()
    {
        float timeout = 5f; // 5초 타임아웃
        float elapsed = 0f;
        
        while (GameStateManager.Instance == null && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        if (GameStateManager.Instance != null && !isEventSubscribed)
        {
            GameStateManager.Instance.OnHumanityChanged += UpdateHumanityDisplay;
            isEventSubscribed = true;
            UpdateHumanityDisplay(GameStateManager.Instance.GetHumanity());
        }
        else if (GameStateManager.Instance == null)
        {
            Debug.LogError("[HumanityText] GameStateManager.Instance를 찾을 수 없습니다. (타임아웃)");
>>>>>>> Stashed changes
        }
    }
    
    private void OnDisable()
    {
<<<<<<< Updated upstream
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnHumanityChanged -= UpdateHumanityDisplay;
=======
        if (GameStateManager.Instance != null && isEventSubscribed)
        {
            GameStateManager.Instance.OnHumanityChanged -= UpdateHumanityDisplay;
            isEventSubscribed = false;
>>>>>>> Stashed changes
        }
    }
    
    private void UpdateHumanityDisplay(float newValue)
    {
        // 텍스트 업데이트
        if (humanityText != null)
        {
            humanityText.text = $"인간성: {newValue:F1}%";
            
            // 색상 변경
            humanityText.color = GetColorByHumanity(newValue);
        }
    }
    
    private Color GetColorByHumanity(float value)
    {
        if (value <= warningThreshold)
            return lowHumanityColor;
        else if (value <= 50f)
            return midHumanityColor;
        else
            return highHumanityColor;
    }
}
