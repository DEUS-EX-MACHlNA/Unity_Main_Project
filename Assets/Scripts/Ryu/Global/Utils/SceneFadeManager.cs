using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 씬 전환 시 페이드 인/아웃 효과를 담당합니다.
/// Canvas에 검은색 Image 오브젝트가 필요합니다.
/// 모든 설정은 스크립트를 통해서만 처리됩니다.
/// </summary>
public class SceneFadeManager : MonoBehaviour
{
    // Inspector에서 설정 불가능 - 스크립트를 통해서만 찾아짐
    private Image fadeImage;
    
    // 페이드 기본 지속 시간 (초)
    private const float DEFAULT_FADE_DURATION = 1f;

    private void Awake()
    {
        // FadeImage 자동 찾기 (스크립트를 통해서만 처리)
        GameObject imageObj = GameObject.Find("FadeImage");
        if (imageObj == null)
        {
            imageObj = GameObject.Find("FadePanel");
        }
        
        if (imageObj != null)
        {
            fadeImage = imageObj.GetComponent<Image>();
            if (fadeImage != null)
            {
                // FadeImage의 초기 색상 확인 및 검은색으로 강제 설정
                Debug.Log($"[SceneFadeManager] FadeImage 찾음: {imageObj.name}, 초기 색상: {fadeImage.color}");
                
                // 초기 색상이 검은색이 아니면 강제로 검은색으로 설정 (노란색 문제 해결)
                if (fadeImage.color.r != 0 || fadeImage.color.g != 0 || fadeImage.color.b != 0)
                {
                    Debug.LogWarning($"[SceneFadeManager] FadeImage의 초기 색상이 검은색이 아닙니다 (R:{fadeImage.color.r}, G:{fadeImage.color.g}, B:{fadeImage.color.b}). 검은색으로 강제 설정합니다.");
                    fadeImage.color = new Color(0, 0, 0, fadeImage.color.a);
                }
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
            StartCoroutine(FadeIn(DEFAULT_FADE_DURATION));
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
    /// RGB는 항상 검은색(0,0,0)으로 강제 설정하여 노란색 문제 해결
    /// </summary>
    private IEnumerator FadeOut(float duration)
    {
        if (fadeImage == null) yield break;

        float elapsed = 0f;
        float startAlpha = fadeImage.color.a;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, 1f, elapsed / duration);
            // RGB를 명시적으로 검은색(0,0,0)으로 설정
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        
        // 최종 색상을 검은색으로 확실히 설정
        fadeImage.color = new Color(0, 0, 0, 1);
        Debug.Log($"[SceneFadeManager] 페이드 아웃 완료. FadeImage 색상: {fadeImage.color}");
    }

    /// <summary>
    /// 페이드 인 효과 (검은색 → 투명)
    /// RGB는 항상 검은색(0,0,0)으로 강제 설정하여 노란색 문제 해결
    /// </summary>
    private IEnumerator FadeIn(float duration)
    {
        if (fadeImage == null) yield break;

        float elapsed = 0f;
        float startAlpha = fadeImage.color.a;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, 0f, elapsed / duration);
            // RGB를 명시적으로 검은색(0,0,0)으로 설정
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        
        // 최종 색상을 투명하게 설정
        fadeImage.color = new Color(0, 0, 0, 0);
        Debug.Log($"[SceneFadeManager] 페이드 인 완료. FadeImage 색상: {fadeImage.color}");
    }
}


