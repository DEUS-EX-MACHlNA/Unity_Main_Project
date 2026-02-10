using UnityEngine;

/// <summary>
/// SpriteRenderer가 있는 오브젝트에 부착하면
/// 마우스 Hover 시 스프라이트가 밝게 빛나는 효과를 줍니다.
/// BoxCollider2D가 필요합니다.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class HoverGlow : MonoBehaviour
{
    [Header("Glow Settings")]
    [Tooltip("Hover 시 밝기 배율 (1보다 크면 밝아짐)")]
    [SerializeField] private float glowMultiplier = 1.5f;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isHovered = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
    }

    private void OnMouseEnter()
    {
        if (isHovered) return;
        isHovered = true;

        Color glowColor = new Color(
            originalColor.r * glowMultiplier,
            originalColor.g * glowMultiplier,
            originalColor.b * glowMultiplier,
            originalColor.a
        );
        spriteRenderer.color = glowColor;

        Debug.Log($"[HoverGlow] {gameObject.name} Hover 시작");
    }

    private void OnMouseExit()
    {
        if (!isHovered) return;
        isHovered = false;

        spriteRenderer.color = originalColor;

        Debug.Log($"[HoverGlow] {gameObject.name} Hover 종료");
    }

    private void OnDisable()
    {
        // 비활성화 시 원래 색상으로 복원
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
            isHovered = false;
        }
    }
}
