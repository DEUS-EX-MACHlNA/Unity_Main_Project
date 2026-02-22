using UnityEngine;

/// <summary>
/// Kitchen 씬의 Background 오브젝트에 부착합니다.
/// 새엄마(NewMother)가 무력화되면 배경 스프라이트를 '주방_새엄마없음'으로 바꾸고,
/// 무력화 해제 시 원래 스프라이트·크기로 복원합니다.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class KitchenBackgroundSwapper : MonoBehaviour
{
    [Header("새엄마 없을 때 배경")]
    [Tooltip("새엄마 무력화 시 표시할 스프라이트 (예: Assets/Assets/드림코어/Backgrounds/주방_새엄마없음.png)")]
    [SerializeField] private Sprite noStepmotherSprite;

    [Header("크기 맞춤")]
    [Tooltip("스프라이트 교체 시 카메라 뷰에 맞춰 크기·비율을 자동 조정합니다.")]
    [SerializeField] private bool fitToCamera = true;

    private SpriteRenderer spriteRenderer;
    private Sprite originalSprite;
    private Vector3 originalScale;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalSprite = spriteRenderer.sprite;
            originalScale = transform.localScale;
        }
    }

    private void Start()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnNPCStatusChanged += OnNPCStatusChanged;
            // 씬 로드 시점에 이미 새엄마가 무력화된 상태일 수 있음 (이벤트는 이미 발생한 후). 현재 상태를 한 번 적용.
            NPCStatus current = GameStateManager.Instance.GetNPCStatus(NPCType.NewMother);
            if (current != null)
                ApplyBackgroundForStatus(current);
        }
    }

    private void OnDisable()
    {
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.OnNPCStatusChanged -= OnNPCStatusChanged;
    }

    private void OnNPCStatusChanged(NPCType npc, NPCStatus status)
    {
        if (npc != NPCType.NewMother || spriteRenderer == null)
            return;
        ApplyBackgroundForStatus(status);
    }

    private void ApplyBackgroundForStatus(NPCStatus status)
    {
        if (spriteRenderer == null) return;

        if (status.isDisabled && noStepmotherSprite != null)
        {
            spriteRenderer.sprite = noStepmotherSprite;
            if (fitToCamera)
                FitSpriteToCamera(noStepmotherSprite);
        }
        else if (!status.isDisabled && originalSprite != null)
        {
            spriteRenderer.sprite = originalSprite;
            if (fitToCamera)
                transform.localScale = originalScale;
        }
    }

    /// <summary>
    /// 스프라이트가 카메라 뷰를 가리도록 크기·비율을 맞춥니다.
    /// </summary>
    private void FitSpriteToCamera(Sprite sprite)
    {
        Camera cam = Camera.main;
        if (cam == null || sprite == null) return;

        float camHeight = 2f * cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        // 스프라이트 월드 크기 (bounds는 pivot 반영된 크기)
        Bounds bounds = sprite.bounds;
        float spriteW = bounds.size.x;
        float spriteH = bounds.size.y;
        if (spriteW <= 0f || spriteH <= 0f) return;

        // 뷰를 꽉 채우도록 스케일 (비율 유지, 커버)
        float scaleX = camWidth / spriteW;
        float scaleY = camHeight / spriteH;
        float scale = Mathf.Max(scaleX, scaleY);

        transform.localScale = new Vector3(scale, scale, transform.localScale.z);
    }

#if UNITY_EDITOR
    [ContextMenu("스프라이트 경로에서 로드 (주방_새엄마없음)")]
    private void EditorLoadNoStepmotherSprite()
    {
        var sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(
            "Assets/Assets/드림코어/Backgrounds/주방_새엄마없음.png");
        if (sprite != null)
        {
            noStepmotherSprite = sprite;
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
#endif
}
