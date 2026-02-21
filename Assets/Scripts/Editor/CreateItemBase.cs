using UnityEngine;
using UnityEditor;

/// <summary>
/// Item_Base 프리팹을 생성하는 Editor 스크립트
/// </summary>
public class CreateItemBase : EditorWindow
{
    [MenuItem("Tools/Item/Create Item Base Prefab")]
    public static void ShowWindow()
    {
        CreateItemBasePrefab();
    }

    private static void CreateItemBasePrefab()
    {
        // GameObject 생성
        GameObject itemBase = new GameObject("Item_Base");
        
        // 컴포넌트 추가
        SpriteRenderer spriteRenderer = itemBase.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = null; // 기본값: None
        spriteRenderer.sortingOrder = 1; // Item은 sortingOrder 1
        
        BoxCollider2D boxCollider = itemBase.AddComponent<BoxCollider2D>();
        boxCollider.size = new Vector2(0.0001f, 0.0001f); // 기본값
        
        HoverGlow hoverGlow = itemBase.AddComponent<HoverGlow>();
        // glowMultiplier는 기본값 1.5 사용
        
        ClickableObject clickable = itemBase.AddComponent<ClickableObject>();
        // blockName은 빈 문자열 (기본값)
        
        // 프리팹으로 저장
        string prefabPath = "Assets/Assets/Prefabs/Items/Item_Base.prefab";
        
        // 기존 프리팹이 있으면 덮어쓰기
        GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (existingPrefab != null)
        {
            if (EditorUtility.DisplayDialog("프리팹 덮어쓰기", 
                $"'{prefabPath}'에 이미 프리팹이 있습니다. 덮어쓰시겠습니까?", 
                "덮어쓰기", "취소"))
            {
                // 기존 프리팹을 새로 생성된 GameObject로 교체
                PrefabUtility.SaveAsPrefabAsset(itemBase, prefabPath);
                DestroyImmediate(itemBase);
                Debug.Log($"Item_Base 프리팹이 업데이트되었습니다: {prefabPath}");
            }
            else
            {
                DestroyImmediate(itemBase);
                Debug.Log("프리팹 생성이 취소되었습니다.");
                return;
            }
        }
        else
        {
            // 새 프리팹 생성
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(itemBase, prefabPath);
            DestroyImmediate(itemBase);
            Debug.Log($"Item_Base 프리팹이 생성되었습니다: {prefabPath}");
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        // 생성된 프리팹 선택
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        EditorGUIUtility.PingObject(Selection.activeObject);
    }
}


