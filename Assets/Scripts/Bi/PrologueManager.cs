using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PrologueManager : MonoBehaviour
{
    [Header("Video")]
    public VideoPlayer videoPlayer;
    public RawImage videoImage;
    public float videoFadeOutDuration = 2.0f;

    [Header("Text")]
    public TextMeshProUGUI textUI;
    public float fadeDuration = 2.0f;
    public float fadeOutDuration = 0.8f;

    [Header("Overlay")]
    public Image fadeOverlay;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip knockingSound;

    private string[] paragraphs = {
        "<color=#C8C7B4>어느 날부터 사람들이 잠들기 시작했습니다.\n\n그리고 깨어나지 않았습니다.\n\n의학계는 이것을 <b>N.O.A.H.</b>라 불렀습니다.\n\nNon-specific Occurrence of Atypical Hypnosia.\n\n비전형적 최면 장애.\n\n약도 소용없었습니다. 전기 자극도, 물리적 충격도.\n외부에서 할 수 있는 건 아무것도 없었습니다.\n\n환자는 오직 스스로만 깨어날 수 있었습니다.</color>",
        "꿈속에는 무언가가 있습니다.\n\n각자가 가장 깊숙이 묻어둔 것.\n절대 꺼내지 않으려 했던 것.\n\nN.O.A.H.는 그것을 끄집어내 형체를 만들고, 환자를 그 앞에 세웁니다.\n\n도망치면 루프는 다시 시작됩니다.\n\n눈을 돌리면 루프는 다시 시작됩니다.\n\n그것을 똑바로 마주하고, 근원을 해소해야만 비로소 눈이 열립니다.\n\n실패하면.\n심장은 뛰지만, 그 사람은 돌아오지 않습니다.",
        "지금, 누군가가 잠들었습니다.\n\n당신은 그 꿈속으로 들어가야 합니다.\n\n그리고 기억하십시오.\n그 꿈은 당신의 것이 아닙니다.\n\n\n하지만 당신의 공포는, 어디서든 따라옵니다.",
        "<color=red><size=80>지금도 복도 끝에서 누군가 문을 두드리고 있습니다.</size></color>"
    };

    private bool skipRequested = false;

    void Start()
    {
        fadeOverlay.color = new Color(0, 0, 0, 0);
        textUI.color = new Color(1, 1, 1, 0);
        textUI.text = "";
        StartCoroutine(PlayIntro());
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            skipRequested = true;
    }

    // ─────────────────────────────────────────
    // 1. 영상 재생 & 암전
    // ─────────────────────────────────────────
    IEnumerator PlayIntro()
    {
        videoPlayer.isLooping = false;
        videoImage.color = new Color(1, 1, 1, 1);
        videoPlayer.Prepare();
        yield return new WaitUntil(() => videoPlayer.isPrepared);
        videoPlayer.Play();

        yield return new WaitUntil(() => videoPlayer.frame >= (long)videoPlayer.frameCount - 2);

        float elapsed = 0f;
        while (elapsed < videoFadeOutDuration)
        {
            elapsed += Time.deltaTime;
            fadeOverlay.color = new Color(0, 0, 0, Mathf.Clamp01(elapsed / videoFadeOutDuration));
            yield return null;
        }

        fadeOverlay.color = new Color(0, 0, 0, 1);
        videoImage.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.5f);
        StartCoroutine(PlayPrologue());
    }

    // ─────────────────────────────────────────
    // 2. 프롤로그 텍스트
    // ─────────────────────────────────────────
    IEnumerator PlayPrologue()
    {
        for (int i = 0; i < paragraphs.Length; i++)
        {
            skipRequested = false;
            textUI.text = "";

            if (i == paragraphs.Length - 1)
            {
                // 마지막 문단 → 타이핑 효과 + 노크 소리
                textUI.color = new Color(1, 1, 1, 1);
                yield return StartCoroutine(TypeText(paragraphs[i]));
            }
            else
            {
                // 나머지 → 페이드 인
                textUI.text = paragraphs[i];
                textUI.color = new Color(1, 1, 1, 0);

                float elapsed = 0f;
                while (elapsed < fadeDuration)
                {
                    if (skipRequested)
                    {
                        textUI.color = new Color(1, 1, 1, 1);
                        skipRequested = false;
                        break;
                    }
                    elapsed += Time.deltaTime;
                    textUI.color = new Color(1, 1, 1, Mathf.Clamp01(elapsed / fadeDuration));
                    yield return null;
                }
            }

            // 클릭 대기
            skipRequested = false;
            yield return new WaitForEndOfFrame();
            yield return new WaitUntil(() => skipRequested);
            skipRequested = false;

            // 페이드 아웃
            if (i == paragraphs.Length - 1)
            {
                yield return StartCoroutine(FadeOutText());
                yield return new WaitForSeconds(0.5f);
                SceneManager.LoadScene("PlayersRoom");
            }
            else
            {
                yield return StartCoroutine(FadeOutText());
            }
        }
    }

    // ─────────────────────────────────────────
    // 3. 타이핑 효과
    // ─────────────────────────────────────────
    IEnumerator TypeText(string text)
    {
        textUI.text = text;
        textUI.maxVisibleCharacters = 0;
        
        if (audioSource != null && knockingSound != null)
        {
            audioSource.clip = knockingSound;
            audioSource.Play();
        }

        int totalChars = textUI.textInfo.characterCount;
        for (int i = 0; i <= totalChars; i++)
        {
            if (skipRequested)
            {
                textUI.maxVisibleCharacters = totalChars;
                skipRequested = false;
                break;
            }
            textUI.maxVisibleCharacters = i;
            yield return new WaitForSeconds(0.05f);
        }

        textUI.maxVisibleCharacters = 99999;
    }

    // ─────────────────────────────────────────
    // 4. 텍스트 페이드 아웃
    // ─────────────────────────────────────────
    IEnumerator FadeOutText()
    {
        float elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            textUI.color = new Color(1, 1, 1, 1f - Mathf.Clamp01(elapsed / fadeOutDuration));
            yield return null;
        }
        textUI.color = new Color(1, 1, 1, 0);
    }
}