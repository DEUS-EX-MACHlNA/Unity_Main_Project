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
    private BoxCollider2D boxCollider;
    private Color originalColor;
    private bool isHovered = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        originalColor = spriteRenderer.color;
        
        // Box Collider 2D 크기를 Sprite 크기에 자동으로 맞춤
        AutoSizeCollider();
    }

    /// <summary>
    /// Box Collider 2D 크기를 Sprite 크기에 자동으로 맞춥니다.
    /// </summary>
    private void AutoSizeCollider()
    {
        if (spriteRenderer == null || boxCollider == null) return;
        if (spriteRenderer.sprite == null)
        {
            Debug.LogWarning($"[HoverGlow] {gameObject.name}: Sprite가 할당되지 않아 Collider 크기를 설정할 수 없습니다.");
            return;
        }

        // Sprite의 bounds를 가져와서 크기 설정
        Bounds spriteBounds = spriteRenderer.sprite.bounds;
        boxCollider.size = spriteBounds.size;
        
        Debug.Log($"[HoverGlow] {gameObject.name} Collider 크기 자동 설정: {boxCollider.size}");
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
