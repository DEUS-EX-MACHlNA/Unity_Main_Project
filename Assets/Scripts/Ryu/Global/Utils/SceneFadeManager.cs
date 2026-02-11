using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 씬 전환 시 페이드 인/아웃 효과를 담당합니다.
/// Canvas에 검은색 Image 오브젝트가 필요합니다.
/// </summary>
public class SceneFadeManager : MonoBehaviour
{
    [Header("Fade Settings")]
    [SerializeField] private Image fadeImage;
    // defaultFadeDuration 필드는 현재 사용되지 않으므로 제거됨

    private void Awake()
    {
        // Inspector에서 설정되지 않은 경우 자동으로 찾기
        if (fadeImage == null)
        {
            // 프로젝트에서 FadeImage / FadePanel 등의 이름을 혼용할 수 있어 둘 다 시도
            GameObject imageObj = GameObject.Find("FadeImage");
            if (imageObj == null)
            {
                imageObj = GameObject.Find("FadePanel");
            }
            if (imageObj != null)
            {
                fadeImage = imageObj.GetComponent<Image>();
            }
        }

        if (fadeImage == null)
        {
            Debug.LogWarning("[SceneFadeManager] fadeImage를 찾지 못했습니다. (FadeImage/FadePanel) 페이드 효과가 동작하지 않을 수 있습니다.");
        }
    }

    private void Start()
    {
        // 씬 로드 후 페이드 인 효과 자동 실행
        if (fadeImage != null && fadeImage.color.a > 0)
        {
            StartCoroutine(FadeIn(1f));
        }
    }

    /// <summary>
    /// 페이드 아웃 → 씬 로드 → 페이드 인을 순차적으로 실행합니다.
    /// </summary>
    public void LoadSceneWithFade(string sceneName, float fadeDuration)
    {
        // 중요: 로드 불가 씬이면 검은 화면에 '갇히는' 현상을 방지하기 위해 시작 자체를 막습니다.
        if (!CanLoadScene(sceneName))
        {
            Debug.LogError($"[SceneFadeManager] 씬 로드 실패: '{sceneName}' (Build Profiles/Shared scene list에 씬이 등록되어 있어야 합니다)");
            return;
        }
        StartCoroutine(LoadSceneWithFadeCoroutine(sceneName, fadeDuration));
    }

    private IEnumerator LoadSceneWithFadeCoroutine(string sceneName, float fadeDuration)
    {
        Debug.Log($"[SceneFadeManager] 페이드 아웃 시작 → {sceneName} 씬으로 전환");
        
        // 페이드 아웃
        yield return StartCoroutine(FadeOut(fadeDuration));
        
        Debug.Log($"[SceneFadeManager] {sceneName} 씬 로드 중...");
        
        // 씬 로드 (새 씬은 검은색 배경으로 시작하지만 Start()에서 자동으로 페이드 인됨)
        SceneManager.LoadScene(sceneName);

        // 만약 씬 로드가 실패하면(예: Build Profiles 미등록), 이 코루틴은 계속 살아있고 화면은 검은색(알파 1)로 남습니다.
        // 다음 프레임에 씬이 바뀌지 않았다면, 검은 화면에 갇히는 현상을 막기 위해 페이드 인으로 복구합니다.
        yield return null;
        if (SceneManager.GetActiveScene().name != sceneName)
        {
            Debug.LogError($"[SceneFadeManager] 씬 전환에 실패했습니다: '{sceneName}'. 검은 화면 복구를 위해 페이드 인합니다.");
            yield return StartCoroutine(FadeIn(fadeDuration));
        }
    }

    private static bool CanLoadScene(string sceneName)
    {
        // 씬이 Build Profiles/Build Settings에 포함되어 있지 않으면 false가 됩니다.
        // (Addressables/AssetBundle로 로드하는 경우는 예외지만, 현재 프로젝트 흐름은 기본 LoadScene 기반입니다.)
        return Application.CanStreamedLevelBeLoaded(sceneName);
    }

    /// <summary>
    /// 페이드 아웃 효과 (투명 → 검은색)
    /// </summary>
    private IEnumerator FadeOut(float duration)
    {
        if (fadeImage == null) yield break;

        float elapsed = 0f;
        Color startColor = fadeImage.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / duration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        
        fadeImage.color = new Color(0, 0, 0, 1);
    }

    /// <summary>
    /// 페이드 인 효과 (검은색 → 투명)
    /// </summary>
    private IEnumerator FadeIn(float duration)
    {
        if (fadeImage == null) yield break;

        float elapsed = 0f;
        Color startColor = fadeImage.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - Mathf.Clamp01(elapsed / duration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        
        fadeImage.color = new Color(0, 0, 0, 0);
    }
}


