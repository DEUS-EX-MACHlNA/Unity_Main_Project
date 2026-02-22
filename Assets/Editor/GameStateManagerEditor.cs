using UnityEditor;
using UnityEngine;

/// <summary>
/// 플레이 모드에서 GameStateManager Inspector에 NPC 상태를 표시합니다.
/// </summary>
[CustomEditor(typeof(GameStateManager))]
public class GameStateManagerEditor : Editor
{
    private static readonly NPCType[] NpcTypesToShow =
    {
        NPCType.NewMother,
        NPCType.NewFather,
        NPCType.Sibling,
        NPCType.Dog,
        NPCType.Grandmother
    };

    private static readonly GUIContent HeaderNpcStates = new GUIContent("NPC 상태 (Runtime)");
    private static readonly GUIContent LabelAffection = new GUIContent("호감도");
    private static readonly GUIContent LabelHumanity = new GUIContent("인간성");
    private static readonly GUIContent LabelAvailable = new GUIContent("대화가능");
    private static readonly GUIContent LabelDisabled = new GUIContent("무력화");
    private static readonly GUIContent LabelRemainingTurns = new GUIContent("남은 턴");
    private static readonly GUIContent LabelReason = new GUIContent("사유");
    private static readonly GUIContent LabelLocation = new GUIContent("위치");

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (!Application.isPlaying)
            return;

        GameStateManager gsm = GameStateManager.Instance;
        if (gsm == null)
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.HelpBox("GameStateManager.Instance가 없습니다. 플레이 모드에서 씬에 GameStateManager가 있는지 확인하세요.", MessageType.Info);
            return;
        }

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField(HeaderNpcStates, EditorStyles.boldLabel);

        foreach (NPCType npcType in NpcTypesToShow)
        {
            NPCStatus status = gsm.GetNPCStatus(npcType);
            GameLocation location = gsm.GetNPCLocation(npcType);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField(NpcTypeLabel(npcType), EditorStyles.boldLabel);

            if (status != null)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField(LabelAffection, new GUIContent(status.affection.ToString("F1")));
                EditorGUILayout.LabelField(LabelHumanity, new GUIContent(status.humanity.ToString("F1")));
                EditorGUILayout.LabelField(LabelAvailable, new GUIContent(status.isAvailable ? "예" : "아니오"));
                EditorGUILayout.LabelField(LabelDisabled, new GUIContent(status.isDisabled ? "예" : "아니오"));
                if (status.isDisabled)
                {
                    EditorGUILayout.LabelField(LabelRemainingTurns, new GUIContent(status.disabledRemainingTurns.ToString()));
                    if (!string.IsNullOrEmpty(status.disabledReason))
                        EditorGUILayout.LabelField(LabelReason, new GUIContent(status.disabledReason));
                }
                EditorGUILayout.LabelField(LabelLocation, new GUIContent(location.ToString()));
                EditorGUI.indentLevel--;
            }
            else
            {
                EditorGUILayout.LabelField("(상태 없음)");
            }

            EditorGUILayout.EndVertical();
        }
    }

    private static string NpcTypeLabel(NPCType npcType)
    {
        switch (npcType)
        {
            case NPCType.NewMother: return "새엄마 (엘리노어)";
            case NPCType.NewFather: return "새아빠 (아더)";
            case NPCType.Sibling: return "동생 (루카스)";
            case NPCType.Dog: return "강아지 (바론)";
            case NPCType.Grandmother: return "할머니 (마가렛)";
            default: return npcType.ToString();
        }
    }
}
