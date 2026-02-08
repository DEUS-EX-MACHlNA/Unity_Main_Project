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
    
    private void OnEnable()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnHumanityChanged += UpdateHumanityDisplay;
            // 초기값 표시
            UpdateHumanityDisplay(GameStateManager.Instance.GetHumanity());
        }
        else
        {
            Debug.LogWarning("[HumanityText] GameStateManager.Instance를 찾을 수 없습니다.");
        }
    }
    
    private void OnDisable()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnHumanityChanged -= UpdateHumanityDisplay;
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
