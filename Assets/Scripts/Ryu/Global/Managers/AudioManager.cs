using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 시간대별 배경음악을 관리하는 싱글톤 매니저입니다.
/// GameStateManager와 연동하여 시간대 변경 시 자동으로 음악을 전환합니다.
/// </summary>
[System.Serializable]
public class BackgroundMusicData
{
    public TimeOfDay timeOfDay;
    public AudioClip musicClip;
    public bool loop = true;
}

/// <summary>
/// 씬별 음악 재생 정책
/// </summary>
public enum MusicPolicy
{
    TimeBased,      // 시간대별 자동 전환
    Fixed,          // 고정 음악
    Stop            // 음악 정지
}

public class AudioManager : MonoBehaviour
{
    // Singleton 인스턴스
    public static AudioManager Instance { get; private set; }

    [Header("Background Music Settings")]
    [SerializeField] private BackgroundMusicData[] musicData;
    [SerializeField] private float defaultVolume = 0.7f;

    [Header("Fade Settings")]
    [SerializeField] private float defaultFadeDuration = 3f;

    private AudioSource audioSource;
    private Dictionary<string, MusicPolicy> sceneMusicPolicies;
    private Dictionary<TimeOfDay, AudioClip> musicDictionary;
    private TimeOfDay currentPlayingTimeOfDay = TimeOfDay.Day;
    
    private const string VOLUME_PREFS_KEY = "BackgroundMusicVolume";
    
    // 페이드 관련 변수
    private Coroutine currentFadeCoroutine;
    private bool isFading = false;

    /// <summary>
    /// 현재 배경음악 볼륨 (0.0 ~ 1.0)
    /// </summary>
    public float MusicVolume { get; private set; }

    /// <summary>
    /// 싱글톤 초기화 및 컴포넌트 설정
    /// </summary>
    private void Awake()
    {
        // Singleton 패턴
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // AudioSource 컴포넌트 가져오기 또는 추가
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            // AudioSource 기본 설정
            audioSource.playOnAwake = false;
            audioSource.loop = true;
            
            // 음악 데이터 초기화
            InitializeMusicData();
            
            // 씬별 정책 초기화
            InitializeScenePolicies();
            
            // 볼륨 로드
            LoadVolume();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    /// <summary>
    /// GameStateManager 이벤트 구독 및 초기 음악 재생
    /// </summary>
    private void Start()
    {
        // GameStateManager가 이미 초기화되었는지 확인
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnTimeOfDayChanged += HandleTimeOfDayChanged;
            
            // 현재 시간대에 맞는 음악 재생
            TimeOfDay currentTime = GameStateManager.Instance.GetCurrentTimeOfDay();
            PlayMusicForTimeOfDay(currentTime);
        }
        else
        {
            // GameStateManager가 아직 초기화되지 않은 경우
            // 현재 씬의 정책에 따라 처리
            HandleSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }
        
        // 씬 전환 이벤트 구독
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    /// <summary>
    /// 이벤트 구독 해제
    /// </summary>
    private void OnDestroy()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnTimeOfDayChanged -= HandleTimeOfDayChanged;
        }
        
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    /// <summary>
    /// 음악 데이터를 Dictionary로 변환하여 초기화
    /// </summary>
    private void InitializeMusicData()
    {
        musicDictionary = new Dictionary<TimeOfDay, AudioClip>();
        
        if (musicData != null)
        {
            foreach (var data in musicData)
            {
                if (data != null && data.musicClip != null)
                {
                    musicDictionary[data.timeOfDay] = data.musicClip;
                }
            }
        }
        
        Debug.Log($"[AudioManager] 음악 데이터 초기화 완료: {musicDictionary.Count}개");
    }

    /// <summary>
    /// 씬별 음악 재생 정책 초기화
    /// </summary>
    private void InitializeScenePolicies()
    {
        sceneMusicPolicies = new Dictionary<string, MusicPolicy>
        {
            // 게임플레이 씬: 시간대별 자동 전환
            { "PlayersRoom", MusicPolicy.TimeBased },
            { "Hallway", MusicPolicy.TimeBased },
            { "LivingRoom", MusicPolicy.TimeBased },
            { "Kitchen", MusicPolicy.TimeBased },
            { "SiblingsRoom", MusicPolicy.TimeBased },
            { "Basement", MusicPolicy.TimeBased },
            { "Backyard", MusicPolicy.TimeBased },
            
            // 메뉴/타이틀 씬: 음악 정지
            { "Title", MusicPolicy.Stop },
            { "GameStart", MusicPolicy.Stop },
            
            // 밤 대화 씬: Night 음악 고정 재생
            { "Night", MusicPolicy.Fixed },
            
            // 게임오버 씬: 고정 음악 (향후 확장 가능)
            { "GameOver", MusicPolicy.Fixed }
        };
        
        Debug.Log($"[AudioManager] 씬별 정책 초기화 완료: {sceneMusicPolicies.Count}개");
    }

    /// <summary>
    /// 시간대 변경 이벤트 핸들러
    /// </summary>
    private void HandleTimeOfDayChanged(TimeOfDay timeOfDay)
    {
        Debug.Log($"[AudioManager] 시간대 변경 감지: {timeOfDay}");
        
        // 현재 씬의 정책 확인
        string currentSceneName = SceneManager.GetActiveScene().name;
        if (sceneMusicPolicies.ContainsKey(currentSceneName))
        {
            MusicPolicy policy = sceneMusicPolicies[currentSceneName];
            
            if (policy == MusicPolicy.TimeBased)
            {
                PlayMusicForTimeOfDay(timeOfDay);
            }
        }
    }

    /// <summary>
    /// 씬 로드 이벤트 핸들러
    /// </summary>
    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string sceneName = scene.name;
        Debug.Log($"[AudioManager] 씬 로드: {sceneName}");
        
        // 씬별 정책 확인
        if (sceneMusicPolicies.ContainsKey(sceneName))
        {
            MusicPolicy policy = sceneMusicPolicies[sceneName];
            
            switch (policy)
            {
                case MusicPolicy.TimeBased:
                    // 시간대별 자동 전환
                    if (GameStateManager.Instance != null)
                    {
                        TimeOfDay currentTime = GameStateManager.Instance.GetCurrentTimeOfDay();
                        PlayMusicForTimeOfDay(currentTime);
                    }
                    else
                    {
                        // GameStateManager가 없으면 기본값으로 Day 음악 재생
                        PlayMusicForTimeOfDay(TimeOfDay.Day);
                    }
                    break;
                    
                case MusicPolicy.Fixed:
                    // 고정 음악
                    // Night 씬은 항상 Night 음악 재생
                    if (sceneName == "Night")
                    {
                        PlayMusicForTimeOfDay(TimeOfDay.Night);
                    }
                    else if (GameStateManager.Instance != null)
                    {
                        // 다른 Fixed 정책 씬은 현재 시간대에 맞는 음악 재생
                        TimeOfDay currentTime = GameStateManager.Instance.GetCurrentTimeOfDay();
                        PlayMusicForTimeOfDay(currentTime);
                    }
                    break;
                    
                case MusicPolicy.Stop:
                    // 음악 정지
                    StopMusic();
                    break;
            }
        }
        else
        {
            // 정책이 정의되지 않은 씬은 기본적으로 음악 정지
            Debug.LogWarning($"[AudioManager] 씬 '{sceneName}'에 대한 정책이 정의되지 않았습니다. 음악을 정지합니다.");
            StopMusic();
        }
    }

    /// <summary>
    /// 특정 시간대에 맞는 음악을 재생합니다.
    /// </summary>
    /// <param name="timeOfDay">재생할 시간대</param>
    /// <param name="useFade">페이드 효과 사용 여부 (기본값: true)</param>
    /// <param name="fadeDuration">페이드 지속 시간 (기본값: -1이면 defaultFadeDuration 사용)</param>
    public void PlayMusicForTimeOfDay(TimeOfDay timeOfDay, bool useFade = true, float fadeDuration = -1)
    {
        if (audioSource == null)
        {
            Debug.LogError("[AudioManager] AudioSource가 없습니다.");
            return;
        }
        
        // 같은 시간대의 음악이 이미 재생 중이면 무시
        if (currentPlayingTimeOfDay == timeOfDay && audioSource.isPlaying && !isFading)
        {
            Debug.Log($"[AudioManager] 이미 {timeOfDay} 음악이 재생 중입니다.");
            return;
        }
        
        // 음악 클립 확인
        if (!musicDictionary.ContainsKey(timeOfDay))
        {
            Debug.LogWarning($"[AudioManager] {timeOfDay}에 해당하는 음악이 없습니다.");
            return;
        }
        
        AudioClip clip = musicDictionary[timeOfDay];
        if (clip == null)
        {
            Debug.LogWarning($"[AudioManager] {timeOfDay} 음악 클립이 null입니다.");
            return;
        }
        
        // 기존 페이드 중지
        StopCurrentFade();
        
        // 페이드 지속 시간 설정
        if (fadeDuration < 0)
        {
            fadeDuration = defaultFadeDuration;
        }
        
        // 음악 재생
        audioSource.clip = clip;
        audioSource.loop = true;
        
        if (useFade)
        {
            // 페이드 인으로 재생
            audioSource.volume = 0f;
            audioSource.Play();
            currentFadeCoroutine = StartCoroutine(FadeInCoroutine(MusicVolume, fadeDuration));
            Debug.Log($"[AudioManager] {timeOfDay} 음악 페이드 인 시작: {clip.name} (지속 시간: {fadeDuration}초)");
        }
        else
        {
            // 즉시 재생
            audioSource.volume = MusicVolume;
            audioSource.Play();
            Debug.Log($"[AudioManager] {timeOfDay} 음악 재생 시작: {clip.name}");
        }
        
        currentPlayingTimeOfDay = timeOfDay;
    }

    /// <summary>
    /// 배경음악을 정지합니다.
    /// </summary>
    /// <param name="useFade">페이드 효과 사용 여부 (기본값: true)</param>
    /// <param name="fadeDuration">페이드 지속 시간 (기본값: -1이면 defaultFadeDuration 사용)</param>
    public void StopMusic(bool useFade = true, float fadeDuration = -1)
    {
        if (audioSource == null || !audioSource.isPlaying)
        {
            return;
        }
        
        // 기존 페이드 중지
        StopCurrentFade();
        
        // 페이드 지속 시간 설정
        if (fadeDuration < 0)
        {
            fadeDuration = defaultFadeDuration;
        }
        
        if (useFade)
        {
            // 페이드 아웃으로 정지
            currentFadeCoroutine = StartCoroutine(FadeOutCoroutine(fadeDuration));
            Debug.Log($"[AudioManager] 배경음악 페이드 아웃 시작 (지속 시간: {fadeDuration}초)");
        }
        else
        {
            // 즉시 정지
            audioSource.Stop();
            Debug.Log("[AudioManager] 배경음악 정지");
        }
    }

    /// <summary>
    /// 배경음악 볼륨을 설정합니다.
    /// </summary>
    /// <param name="volume">볼륨 값 (0.0 ~ 1.0)</param>
    public void SetVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        MusicVolume = volume;
        
        // 페이드 중이 아닐 때만 즉시 볼륨 변경
        // 페이드 중이면 MusicVolume만 업데이트하고, 페이드 완료 후 자동으로 적용됨
        if (audioSource != null && !isFading)
        {
            audioSource.volume = volume;
        }
        
        PlayerPrefs.SetFloat(VOLUME_PREFS_KEY, volume);
        PlayerPrefs.Save();
        
        Debug.Log($"[AudioManager] 볼륨 설정: {volume} (페이드 중: {isFading})");
    }

    /// <summary>
    /// 저장된 볼륨 설정을 로드합니다.
    /// </summary>
    private void LoadVolume()
    {
        MusicVolume = PlayerPrefs.GetFloat(VOLUME_PREFS_KEY, defaultVolume);
        if (audioSource != null)
        {
            audioSource.volume = MusicVolume;
        }
        
        Debug.Log($"[AudioManager] 볼륨 로드: {MusicVolume}");
    }

    /// <summary>
    /// 현재 재생 중인 시간대를 반환합니다.
    /// </summary>
    public TimeOfDay GetCurrentPlayingTimeOfDay()
    {
        return currentPlayingTimeOfDay;
    }

    /// <summary>
    /// 배경음악이 재생 중인지 확인합니다.
    /// </summary>
    public bool IsPlaying()
    {
        return audioSource != null && audioSource.isPlaying;
    }

    // ============================================
    // 페이드 효과 관련 메서드
    // ============================================

    /// <summary>
    /// 현재 실행 중인 페이드 코루틴을 중지합니다.
    /// </summary>
    private void StopCurrentFade()
    {
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
            currentFadeCoroutine = null;
        }
        isFading = false;
    }

    /// <summary>
    /// 페이드 인 효과 (볼륨 0 → 목표 볼륨)
    /// </summary>
    /// <param name="targetVolume">목표 볼륨 (0.0 ~ 1.0)</param>
    /// <param name="duration">페이드 지속 시간 (초)</param>
    private IEnumerator FadeInCoroutine(float targetVolume, float duration)
    {
        if (audioSource == null) yield break;

        isFading = true;
        float startVolume = audioSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, t);
            yield return null;
        }

        audioSource.volume = targetVolume;
        isFading = false;
        currentFadeCoroutine = null;
        Debug.Log($"[AudioManager] 페이드 인 완료: 볼륨 {targetVolume}");
    }

    /// <summary>
    /// 페이드 아웃 효과 (현재 볼륨 → 0)
    /// </summary>
    /// <param name="duration">페이드 지속 시간 (초)</param>
    private IEnumerator FadeOutCoroutine(float duration)
    {
        if (audioSource == null) yield break;

        isFading = true;
        float startVolume = audioSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, t);
            yield return null;
        }

        audioSource.volume = 0f;
        audioSource.Stop();
        isFading = false;
        currentFadeCoroutine = null;
        Debug.Log("[AudioManager] 페이드 아웃 완료");
    }
}
