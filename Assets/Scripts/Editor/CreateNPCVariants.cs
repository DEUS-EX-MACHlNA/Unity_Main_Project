using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// NPC 프리팹을 NPC_Base의 Variant로 변환하는 Editor 스크립트
/// </summary>
public class CreateNPCVariants : EditorWindow
{
    private string basePrefabPath = "Assets/Assets/Prefabs/NPCs/NPC_Base.prefab";
    
    [MenuItem("Tools/NPC/Create NPC Variants")]
    public static void ShowWindow()
    {
        GetWindow<CreateNPCVariants>("Create NPC Variants");
    }

    private void OnGUI()
    {
        GUILayout.Label("NPC 프리팹 Variant 생성", EditorStyles.boldLabel);
        
        EditorGUILayout.Space();
        basePrefabPath = EditorGUILayout.TextField("Base Prefab Path:", basePrefabPath);
        
        EditorGUILayout.Space();
        if (GUILayout.Button("Create All Variants"))
        {
            CreateAllVariants();
        }
    }

    private void CreateAllVariants()
    {
        // Base 프리팹 로드
        GameObject basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(basePrefabPath);
        if (basePrefab == null)
        {
            Debug.LogError($"Base prefab을 찾을 수 없습니다: {basePrefabPath}");
            return;
        }

        // 변환할 프리팹 목록
        string[] prefabNames = { "NPC_Sibling", "NPC_Dog", "NPC_Grandmother", "NPC_NewFather", "NPC_NewMother" };
        string[] blockNames = { "Sibling", "Dog", "Grandmother", "NewFather", "NewMother" };
        
        string folderPath = Path.GetDirectoryName(basePrefabPath);

        for (int i = 0; i < prefabNames.Length; i++)
        {
            string oldPrefabPath = $"{folderPath}/{prefabNames[i]}.prefab";
            GameObject oldPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(oldPrefabPath);
            
            if (oldPrefab == null)
            {
                Debug.LogWarning($"프리팹을 찾을 수 없습니다: {oldPrefabPath}");
                continue;
            }

            // Variant 생성
            string variantPath = $"{folderPath}/{prefabNames[i]}.prefab";
            
            // 기존 프리팹을 Variant로 변환
            GameObject variantInstance = PrefabUtility.InstantiatePrefab(basePrefab) as GameObject;
            
            // ClickableObject의 blockName 설정
            ClickableObject clickable = variantInstance.GetComponent<ClickableObject>();
            if (clickable != null)
            {
                // SerializedObject를 사용하여 blockName 설정
                SerializedObject serializedObject = new SerializedObject(clickable);
                SerializedProperty blockNameProperty = serializedObject.FindProperty("blockName");
                if (blockNameProperty != null)
                {
                    blockNameProperty.stringValue = blockNames[i];
                    serializedObject.ApplyModifiedProperties();
                }
            }

            // Variant로 저장 (부모 프리팹 연결)
            GameObject variantPrefab = PrefabUtility.SaveAsPrefabAsset(variantInstance, variantPath);
            
            // 임시 인스턴스 삭제
            DestroyImmediate(variantInstance);
            
            // Variant의 부모를 Base로 설정 (Unity 2018.3+)
            PrefabUtility.SetPropertyModifications(variantPrefab, new PropertyModification[0]);
            PrefabUtility.SavePrefabAsset(variantPrefab);
            
            // 부모 프리팹 연결 (수동으로 설정 필요할 수 있음)
            // Unity Editor에서 수동으로 Variant로 변환하는 것이 더 안전할 수 있습니다.
            
            Debug.Log($"Variant 생성 완료: {variantPath}");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log("모든 Variant 생성이 완료되었습니다!");
    }
}

