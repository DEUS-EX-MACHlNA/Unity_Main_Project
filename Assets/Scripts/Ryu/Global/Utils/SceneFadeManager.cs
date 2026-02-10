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
            GameObject imageObj = GameObject.Find("FadeImage");
            if (imageObj != null)
            {
                fadeImage = imageObj.GetComponent<Image>();
            }
        }

        if (fadeImage != null)
        {
            // 현재 씬 이름 확인
            string currentSceneName = SceneManager.GetActiveScene().name;
            
            // Tutorial 씬과 Night 씬에서는 완전 투명(alpha=0)으로 시작
            if (currentSceneName == "Tutorial" || currentSceneName == "Night")
            {
                fadeImage.color = new Color(0, 0, 0, 0);
            }
            else
            {
                // 다른 씬에서는 검은색 배경으로 유지
                fadeImage.color = new Color(0, 0, 0, 1);
            }
        }
    }

    /// <summary>
    /// 페이드 아웃 → 씬 로드 → 페이드 인을 순차적으로 실행합니다.
    /// </summary>
    public void LoadSceneWithFade(string sceneName, float fadeDuration)
    {
        StartCoroutine(LoadSceneWithFadeCoroutine(sceneName, fadeDuration));
    }

    private IEnumerator LoadSceneWithFadeCoroutine(string sceneName, float fadeDuration)
    {
        Debug.Log($"[SceneFadeManager] 페이드 아웃 시작 → {sceneName} 씬으로 전환");
        
        // 페이드 아웃
        yield return StartCoroutine(FadeOut(fadeDuration));
        
        Debug.Log($"[SceneFadeManager] {sceneName} 씬 로드 중...");
        
        // 씬 로드 (새 씬은 검은색 배경으로 시작)
        SceneManager.LoadScene(sceneName);
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


