using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 씬의 특정 영역을 클릭하면 다른 씬으로 전환하는 컴포넌트입니다.
/// PolygonCollider2D가 필요합니다.
/// </summary>
[RequireComponent(typeof(PolygonCollider2D))]
public class SceneTransitionArea : MonoBehaviour
{
    [Header("Scene Transition Settings")]
    [Tooltip("전환할 목적지 씬 이름 (Build Settings에 등록된 씬 이름)")]
    [SerializeField] private string targetSceneName;
    
    [Tooltip("페이드 아웃 지속 시간 (초)")]
    [SerializeField] private float fadeOutDuration = 1f;
    
    [Tooltip("페이드 인 지속 시간 (초)")]
    [SerializeField] private float fadeInDuration = 1f;
    
    [Header("Visual Feedback")]
    [Tooltip("호버 시 하이라이트 색상")]
    [SerializeField] private Color highlightColor = new Color(1f, 1f, 0.5f, 1f);
    
    [Tooltip("하이라이트 효과 사용 여부")]
    [SerializeField] private bool useHighlight = true;
    
    [Tooltip("호버 시 커서 텍스처 (비어있으면 기본 핸드 포인터 사용)")]
    [SerializeField] private Texture2D cursorTexture;
    
    [Tooltip("커서 핫스팟 (클릭 포인트 위치)")]
    [SerializeField] private Vector2 cursorHotspot = Vector2.zero;
    
    [Header("Audio")]
    [Tooltip("클릭 시 재생할 효과음 (선택적, 비어있으면 재생 안 함)")]
    [SerializeField] private AudioClip clickSound;
    
    [Tooltip("효과음 볼륨")]
    [SerializeField] [Range(0f, 1f)] private float soundVolume = 1f;
    
    [Header("Settings")]
    [Tooltip("중복 클릭 방지 쿨다운 (초)")]
    [SerializeField] private float clickCooldown = 0.1f;
    
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private float lastClickTime;
    private bool isHovering = false;
    private Texture2D defaultCursor;
    private Texture2D cpuAccessibleCursorTexture; // CPU 접근 가능한 커서 텍스처
    
    private void Awake()
    {
        // SpriteRenderer 찾기 (있으면 하이라이트 사용)
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        else
        {
            // SpriteRenderer가 없으면 하이라이트 비활성화
            useHighlight = false;
            Debug.LogWarning($"[SceneTransitionArea] {gameObject.name}: SpriteRenderer를 찾을 수 없습니다. 하이라이트 효과가 비활성화됩니다.");
        }
        
        // 현재 커서 저장 (복원용)
        // Unity에서는 현재 커서를 직접 가져올 수 없으므로, null로 초기화
        defaultCursor = null;
        
        // 커서 텍스처를 CPU 접근 가능한 형태로 변환
        if (cursorTexture != null)
        {
            cpuAccessibleCursorTexture = CreateCPUAccessibleTexture(cursorTexture);
        }
    }
    
    /// <summary>
    /// 텍스처를 CPU 접근 가능한 형태로 복사합니다.
    /// Cursor.SetCursor()는 CPU 접근 가능한 텍스처가 필요합니다.
    /// </summary>
    private Texture2D CreateCPUAccessibleTexture(Texture2D sourceTexture)
    {
        if (sourceTexture == null) return null;
        
        try
        {
            // RenderTexture를 사용하여 텍스처를 읽을 수 있는 형태로 변환
            RenderTexture renderTexture = RenderTexture.GetTemporary(
                sourceTexture.width,
                sourceTexture.height,
                0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.Linear);
            
            Graphics.Blit(sourceTexture, renderTexture);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTexture;
            
            // 새로운 CPU 접근 가능한 텍스처 생성
            Texture2D readableTexture = new Texture2D(sourceTexture.width, sourceTexture.height);
            readableTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            readableTexture.Apply();
            
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTexture);
            
            return readableTexture;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SceneTransitionArea] 커서 텍스처 변환 실패: {e.Message}");
            return null;
        }
    }
    
    private void OnMouseEnter()
    {
        isHovering = true;
        
        // 하이라이트 효과
        if (useHighlight && spriteRenderer != null)
        {
            spriteRenderer.color = highlightColor;
        }
        
        // 커서 변경
        ChangeCursor(true);
    }
    
    private void OnMouseExit()
    {
        isHovering = false;
        
        // 하이라이트 복원
        if (useHighlight && spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
        
        // 커서 복원
        ChangeCursor(false);
    }
    
    private void ChangeCursor(bool isHovering)
    {
        if (isHovering)
        {
            // 호버 시 클릭 커서로 변경
            if (cpuAccessibleCursorTexture != null)
            {
                // CPU 접근 가능한 커서 텍스처 사용
                Cursor.SetCursor(cpuAccessibleCursorTexture, cursorHotspot, CursorMode.Auto);
            }
            else
            {
                // 기본 핸드 포인터 커서 (시스템 기본 사용)
                // Unity에서는 기본 핸드 포인터를 직접 제공하지 않으므로
                // 커스텀 텍스처가 없으면 기본 커서 유지
                // 필요시 Resources 폴더에서 기본 커서 텍스처를 로드할 수 있음
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            }
        }
        else
        {
            // 원래 커서로 복원 (기본 커서로 복원)
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }
    
    private void OnDisable()
    {
        // GameObject가 비활성화될 때 커서 복원 (씬 전환 시 호출됨)
        if (isHovering)
        {
            RestoreDefaultCursor();
        }
    }
    
    private void OnDestroy()
    {
        // GameObject가 파괴될 때 커서 복원 및 리소스 정리
        if (isHovering)
        {
            RestoreDefaultCursor();
        }
        
        // 생성한 CPU 접근 가능한 텍스처 정리
        if (cpuAccessibleCursorTexture != null)
        {
            Destroy(cpuAccessibleCursorTexture);
        }
    }
    
    /// <summary>
    /// 커서를 기본값으로 복원합니다.
    /// </summary>
    private void RestoreDefaultCursor()
    {
        isHovering = false;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
    
    private void OnMouseDown()
    {
        // 쿨다운 체크
        if (Time.time - lastClickTime < clickCooldown)
            return;
        
        lastClickTime = Time.time;
        
        // 목적지 씬 이름 확인
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError($"[SceneTransitionArea] {gameObject.name}: 목적지 씬 이름이 설정되지 않았습니다.");
            return;
        }
        
        // 효과음 재생
        PlayClickSound();
        
        // 씬 전환
        TransitionToScene();
    }
    
    private void PlayClickSound()
    {
        if (clickSound == null) return;
        
        // AudioSource.PlayClipAtPoint를 사용하여 효과음 재생
        AudioSource.PlayClipAtPoint(clickSound, transform.position, soundVolume);
        Debug.Log($"[SceneTransitionArea] {gameObject.name}: 클릭 효과음 재생");
    }
    
    private void TransitionToScene()
    {
        Debug.Log($"[SceneTransitionArea] {gameObject.name}: {targetSceneName} 씬으로 전환 시작");
        
        // 씬 전환 직전에 커서 복원 (OnDisable/OnDestroy가 호출되기 전에 확실하게 복원)
        if (isHovering)
        {
            RestoreDefaultCursor();
        }
        
        // SceneFadeManager 찾기
        SceneFadeManager fadeManager = FindFirstObjectByType<SceneFadeManager>();
        
        if (fadeManager != null)
        {
            // 페이드 아웃 후 씬 전환, 페이드 인도 적용
            fadeManager.LoadSceneWithFade(targetSceneName, fadeOutDuration, fadeInDuration);
        }
        else
        {
            // SceneFadeManager가 없으면 페이드 없이 즉시 전환
            Debug.LogWarning($"[SceneTransitionArea] SceneFadeManager를 찾을 수 없습니다. 페이드 없이 씬을 전환합니다.");
            SceneManager.LoadScene(targetSceneName);
        }
    }
}

