using UnityEngine;

/// <summary>
/// 클릭 가능한 오브젝트에 부착하면,
/// 클릭 시 InputField에 @오브젝트명을 삽입합니다.
/// BoxCollider2D가 필요합니다.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class ClickableObject : MonoBehaviour
{
    [Header("Block Settings")]
    [Tooltip("InputField에 삽입될 블록 이름. 비어있으면 GameObject 이름을 사용합니다.")]
    [SerializeField] private string blockName;

    [Header("Cooldown")]
    [Tooltip("중복 클릭 방지 쿨다운 (초)")]
    [SerializeField] private float clickCooldown = 0.1f;

    private InputHandler inputHandler;
    private float lastClickTime;

    private void Start()
    {
        inputHandler = FindFirstObjectByType<InputHandler>();

        if (inputHandler == null)
        {
            Debug.LogError($"[ClickableObject] {gameObject.name}: InputHandler를 찾을 수 없습니다.");
        }

        if (string.IsNullOrEmpty(blockName))
        {
            blockName = gameObject.name;
        }
    }

    private void OnMouseDown()
    {
        if (Time.time - lastClickTime < clickCooldown)
            return;

        lastClickTime = Time.time;

        if (inputHandler != null)
        {
            inputHandler.AddBlockToInput(blockName);
            Debug.Log($"[ClickableObject] {gameObject.name} 클릭 → @{blockName} 삽입");
        }
    }
}
