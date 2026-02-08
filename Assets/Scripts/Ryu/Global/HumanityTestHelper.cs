using UnityEngine;

/// <summary>
/// 테스트를 위한 인간성 수치 설정 헬퍼 스크립트입니다.
/// Inspector에서 버튼을 통해 인간성을 10으로 설정할 수 있습니다.
/// </summary>
public class HumanityTestHelper : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private float testHumanityValue = 10f;

    /// <summary>
    /// Inspector에서 호출할 수 있는 테스트 메서드입니다.
    /// </summary>
    [ContextMenu("Set Humanity to 10")]
    public void SetHumanityTo10()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.SetHumanity(10f);
            Debug.Log("[HumanityTestHelper] 인간성을 10%로 설정했습니다.");
        }
        else
        {
            Debug.LogWarning("[HumanityTestHelper] GameStateManager.Instance를 찾을 수 없습니다.");
        }
    }

    /// <summary>
    /// Inspector에서 설정한 값으로 인간성을 설정합니다.
    /// </summary>
    [ContextMenu("Set Humanity to Test Value")]
    public void SetHumanityToTestValue()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.SetHumanity(testHumanityValue);
            Debug.Log($"[HumanityTestHelper] 인간성을 {testHumanityValue}%로 설정했습니다.");
        }
        else
        {
            Debug.LogWarning("[HumanityTestHelper] GameStateManager.Instance를 찾을 수 없습니다.");
        }
    }
}
