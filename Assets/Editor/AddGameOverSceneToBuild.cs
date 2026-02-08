#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// GameOver 씬을 Build Settings에 자동으로 추가하는 에디터 스크립트
/// </summary>
public class AddGameOverSceneToBuild : EditorWindow
{
    [MenuItem("Tools/Add GameOver Scene to Build Settings")]
    public static void AddSceneToBuild()
    {
        string scenePath = "Assets/Scenes/Ryu/GameOver 1.unity";
        
        // 이미 추가되어 있는지 확인
        var scenes = EditorBuildSettings.scenes;
        foreach (var scene in scenes)
        {
            if (scene.path == scenePath)
            {
                Debug.Log($"[AddGameOverSceneToBuild] {scenePath} is already in Build Settings.");
                return;
            }
        }
        
        // 씬을 Build Settings에 추가
        var newScenes = new EditorBuildSettingsScene[scenes.Length + 1];
        System.Array.Copy(scenes, newScenes, scenes.Length);
        newScenes[scenes.Length] = new EditorBuildSettingsScene(scenePath, true);
        EditorBuildSettings.scenes = newScenes;
        
        Debug.Log($"[AddGameOverSceneToBuild] {scenePath} has been added to Build Settings.");
    }
    
    [InitializeOnLoadMethod]
    static void OnProjectLoadedInEditor()
    {
        // 프로젝트 로드 시 자동으로 실행
        AddSceneToBuild();
    }
}
#endif
