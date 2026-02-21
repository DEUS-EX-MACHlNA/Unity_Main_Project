using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// 문 오브젝트 호버 시 툴팁을 표시하는 매니저입니다.
/// 싱글톤 패턴을 사용하며 DontDestroyOnLoad로 씬 전환 후에도 유지됩니다.
/// </summary>
public class DoorTooltipManager : MonoBehaviour
{
    public static DoorTooltipManager Instance { get; private set; }
    
    [Header("UI References")]
    [Tooltip("툴팁 패널 GameObject")]
    [SerializeField] private GameObject tooltipPanel;
    
    [Tooltip("제목 텍스트 (TextMeshProUGUI)")]
    [SerializeField] private TextMeshProUGUI titleText;
    
    [Tooltip("배경 이미지 (Image)")]
    [SerializeField] private Image backgroundImage;
    
    [Header("Settings")]
    [Tooltip("호버 후 툴팁이 나타나기까지의 지연 시간 (초)")]
    [SerializeField] private float showDelay = 0.3f;
    
    [Tooltip("툴팁과 마우스 커서 사이의 오프셋 (픽셀)")]
    [SerializeField] private Vector2 cursorOffset = new Vector2(20, 20);
    
    [Tooltip("툴팁 배경 색상")]
    [SerializeField] private Color backgroundColor = new Color(0, 0, 0, 0.8f);
    
    private RectTransform tooltipRectTransform;
    private Canvas canvas;
    private Coroutine showTooltipCoroutine;
    private bool isShowing = false;
    
    private void Awake()
    {
        // 싱글톤 설정
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
        
        // 컴포넌트 초기화
        if (tooltipPanel != null)
        {
            tooltipRectTransform = tooltipPanel.GetComponent<RectTransform>();
            tooltipPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("[DoorTooltipManager] TooltipPanel이 설정되지 않았습니다.");
        }
        
        canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("[DoorTooltipManager] Canvas를 찾을 수 없습니다.");
        }
        
        // 배경 색상 설정
        if (backgroundImage != null)
        {
            backgroundImage.color = backgroundColor;
        }
    }
    
    /// <summary>
    /// 툴팁을 표시합니다.
    /// </summary>
    /// <param name="title">문 제목 (예: "거실로 이동")</param>
    public void ShowTooltip(string title)
    {
        if (tooltipPanel == null || titleText == null)
        {
            Debug.LogWarning("[DoorTooltipManager] UI 참조가 설정되지 않았습니다.");
            return;
        }
        
        // 기존 코루틴 중지
        if (showTooltipCoroutine != null)
        {
            StopCoroutine(showTooltipCoroutine);
        }
        
        showTooltipCoroutine = StartCoroutine(ShowTooltipCoroutine(title));
    }
    
    /// <summary>
    /// 툴팁을 숨깁니다.
    /// </summary>
    public void HideTooltip()
    {
        if (showTooltipCoroutine != null)
        {
            StopCoroutine(showTooltipCoroutine);
            showTooltipCoroutine = null;
        }
        
        if (tooltipPanel != null && isShowing)
        {
            StartCoroutine(HideTooltipCoroutine());
        }
    }
    
    private IEnumerator ShowTooltipCoroutine(string title)
    {
        // 지연 시간 대기
        yield return new WaitForSeconds(showDelay);
        
        // 텍스트 업데이트
        titleText.text = title;
        
        // 위치 설정
        UpdateTooltipPosition();
        
        // 툴팁 활성화 및 페이드 인
        tooltipPanel.SetActive(true);
        isShowing = true;
        
        // 페이드 인 애니메이션
        CanvasGroup canvasGroup = tooltipPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = tooltipPanel.AddComponent<CanvasGroup>();
        }
        
        canvasGroup.alpha = 0f;
        float fadeDuration = 0.2f;
        float elapsed = 0f;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
    }
    
    private IEnumerator HideTooltipCoroutine()
    {
        CanvasGroup canvasGroup = tooltipPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            tooltipPanel.SetActive(false);
            isShowing = false;
            yield break;
        }
        
        // 페이드 아웃 애니메이션
        float fadeDuration = 0.15f;
        float elapsed = 0f;
        float startAlpha = canvasGroup.alpha;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeDuration);
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
        tooltipPanel.SetActive(false);
        isShowing = false;
    }
    
    /// <summary>
    /// 마우스 위치에 따라 툴팁 위치를 업데이트합니다.
    /// </summary>
    private void UpdateTooltipPosition()
    {
        if (tooltipRectTransform == null || canvas == null) return;
        
        // Canvas Render Mode에 따라 카메라 설정
        // Screen Space - Overlay 모드에서는 null을 전달해야 함
        Camera cam = null;
        if (canvas.renderMode == RenderMode.ScreenSpaceCamera || 
            canvas.renderMode == RenderMode.WorldSpace)
        {
            cam = canvas.worldCamera;
        }
        // Overlay 모드에서는 cam = null (기본값)
        
        // 마우스 스크린 좌표를 Canvas 좌표로 변환
        Vector2 mousePosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            Input.mousePosition,
            cam,
            out mousePosition);
        
        // 오프셋 적용
        mousePosition += cursorOffset;
        
        // 실제 Canvas 크기 계산
        RectTransform canvasRect = canvas.transform as RectTransform;
        Vector2 canvasSize = canvasRect.sizeDelta;
        
        // Screen Space - Overlay 모드에서 sizeDelta가 (0,0)인 경우 스크린 크기 사용
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay && 
            (canvasSize.x == 0 || canvasSize.y == 0))
        {
            canvasSize = new Vector2(Screen.width, Screen.height);
        }
        
        Vector2 tooltipSize = tooltipRectTransform.sizeDelta;
        
        // 오른쪽 경계 체크
        if (mousePosition.x + tooltipSize.x > canvasSize.x / 2)
        {
            mousePosition.x = canvasSize.x / 2 - tooltipSize.x - cursorOffset.x;
        }
        
        // 위쪽 경계 체크
        if (mousePosition.y + tooltipSize.y > canvasSize.y / 2)
        {
            mousePosition.y = canvasSize.y / 2 - tooltipSize.y - cursorOffset.y;
        }
        
        // 왼쪽 경계 체크
        if (mousePosition.x < -canvasSize.x / 2)
        {
            mousePosition.x = -canvasSize.x / 2 + cursorOffset.x;
        }
        
        // 아래쪽 경계 체크
        if (mousePosition.y < -canvasSize.y / 2)
        {
            mousePosition.y = -canvasSize.y / 2 + cursorOffset.y;
        }
        
        tooltipRectTransform.anchoredPosition = mousePosition;
    }
    
    private void Update()
    {
        // 툴팁이 표시 중일 때 마우스 위치 추적
        if (isShowing && tooltipPanel != null && tooltipPanel.activeSelf)
        {
            UpdateTooltipPosition();
        }
    }
}

