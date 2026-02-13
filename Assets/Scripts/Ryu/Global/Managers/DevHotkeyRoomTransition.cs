using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 개발용: 숫자키(1~7)로 즉시 방(씬) 전환합니다.
/// - 1 PlayersRoom
/// - 2 Hallway
/// - 3 LivingRoom
/// - 4 Kitchen
/// - 5 SiblingsRoom
/// - 6 Basement
/// - 7 Backyard
///
/// 안전장치:
/// - InputField/TMP_InputField에 포커스가 있으면 단축키 무시
/// - Editor 또는 Development Build 에서만 동작
///
/// 배치:
/// - 씬에 따로 배치하지 않아도, 런타임에 자동 생성되어 DontDestroyOnLoad로 유지됩니다.
/// </summary>
public sealed class DevHotkeyRoomTransition : MonoBehaviour
{
    [Header("Fade")]
    [SerializeField] private float fadeDuration = 1f;

    private static DevHotkeyRoomTransition instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (instance != null) return;

        GameObject go = new GameObject(nameof(DevHotkeyRoomTransition));
        DontDestroyOnLoad(go);
        instance = go.AddComponent<DevHotkeyRoomTransition>();
#endif
    }

    private void Awake()
    {
        // 중복 생성 방지
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (IsTypingInInputField())
            return;

        if (WasPressed(KeyCode.Alpha1, KeyCode.Keypad1)) GoToRoom(GameLocation.PlayersRoom);
        else if (WasPressed(KeyCode.Alpha2, KeyCode.Keypad2)) GoToRoom(GameLocation.Hallway);
        else if (WasPressed(KeyCode.Alpha3, KeyCode.Keypad3)) GoToRoom(GameLocation.LivingRoom);
        else if (WasPressed(KeyCode.Alpha4, KeyCode.Keypad4)) GoToRoom(GameLocation.Kitchen);
        else if (WasPressed(KeyCode.Alpha5, KeyCode.Keypad5)) GoToRoom(GameLocation.SiblingsRoom);
        else if (WasPressed(KeyCode.Alpha6, KeyCode.Keypad6)) GoToRoom(GameLocation.Basement);
        else if (WasPressed(KeyCode.Alpha7, KeyCode.Keypad7)) GoToRoom(GameLocation.Backyard);
#endif
    }

    private static bool WasPressed(KeyCode a, KeyCode b)
    {
        return Input.GetKeyDown(a) || Input.GetKeyDown(b);
    }

    private static bool IsTypingInInputField()
    {
        if (EventSystem.current == null) return false;

        GameObject selected = EventSystem.current.currentSelectedGameObject;
        if (selected == null) return false;

        // TMP InputField
        TMP_InputField tmp = selected.GetComponent<TMP_InputField>();
        if (tmp != null && tmp.isFocused) return true;

        // Legacy UI InputField
        InputField ui = selected.GetComponent<InputField>();
        if (ui != null && ui.isFocused) return true;

        return false;
    }

    private void GoToRoom(GameLocation location)
    {
        string sceneName = location.ToString();

        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.SetCurrentLocation(location);
        }

        SceneFadeManager fadeManager = FindFirstObjectByType<SceneFadeManager>();
        if (fadeManager != null)
        {
            fadeManager.LoadSceneWithFade(sceneName, fadeDuration);
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}




