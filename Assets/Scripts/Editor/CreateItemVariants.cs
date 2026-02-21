using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Item 프리팹을 Item_Base의 Variant로 변환하는 Editor 스크립트
/// </summary>
public class CreateItemVariants : EditorWindow
{
    private string basePrefabPath = "Assets/Assets/Prefabs/Items/Item_Base.prefab";
    
    [MenuItem("Tools/Item/Create Item Variants")]
    public static void ShowWindow()
    {
        GetWindow<CreateItemVariants>("Create Item Variants");
    }

    private void OnGUI()
    {
        GUILayout.Label("Item 프리팹 Variant 생성", EditorStyles.boldLabel);
        
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
        string[] prefabNames = { 
            "Item_BrassKey", 
            "Item_EarlGreyTea", 
            "Item_Hole", 
            "Item_OilBottle", 
            "Item_RealFamilyPhoto", 
            "Item_SiblingsToy", 
            "Item_SilverLighter", 
            "Item_SleepingPill" 
        };
        string[] blockNames = { 
            "BrassKey", 
            "EarlGreyTea", 
            "Hole", 
            "OilBottle", 
            "RealFamilyPhoto", 
            "SiblingsToy", 
            "SilverLighter", 
            "SleepingPill" 
        };
        
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

            // 기존 프리팹에서 스프라이트 정보 가져오기
            SpriteRenderer oldSpriteRenderer = oldPrefab.GetComponent<SpriteRenderer>();
            Sprite oldSprite = oldSpriteRenderer != null ? oldSpriteRenderer.sprite : null;

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

            // SpriteRenderer의 스프라이트 설정 (기존 프리팹에 스프라이트가 있었던 경우)
            if (oldSprite != null)
            {
                SpriteRenderer spriteRenderer = variantInstance.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.sprite = oldSprite;
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
        Debug.LogWarning("주의: Unity Editor에서 수동으로 Base 프리팹을 부모로 설정해야 합니다!");
    }
}

