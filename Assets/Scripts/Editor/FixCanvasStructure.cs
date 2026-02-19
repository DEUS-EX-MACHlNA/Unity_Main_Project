using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class FixCanvasStructure : EditorWindow
{
    [MenuItem("Tools/Fix Canvas Structure")]
    public static void FixCanvas()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas를 찾을 수 없습니다.");
            return;
        }

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        if (canvasRect == null)
        {
            Debug.LogError("Canvas에 RectTransform이 없습니다.");
            return;
        }

        // Canvas 위치를 (0, 0, 0)으로 설정
        canvasRect.position = Vector3.zero;
        canvasRect.localPosition = Vector3.zero;

        // Canvas Anchor를 화면 전체에 맞게 설정
        canvasRect.anchorMin = new Vector2(0, 0);
        canvasRect.anchorMax = new Vector2(0, 0);
        canvasRect.anchoredPosition = Vector2.zero;
        canvasRect.sizeDelta = Vector2.zero;

        // DialoguePanel 찾기
        Transform dialoguePanel = canvas.transform.Find("DialoguePanel");
        if (dialoguePanel != null)
        {
            RectTransform panelRect = dialoguePanel.GetComponent<RectTransform>();
            if (panelRect != null)
            {
                // DialoguePanel을 화면 전체에 맞게 설정
                panelRect.anchorMin = new Vector2(0, 0);
                panelRect.anchorMax = new Vector2(1, 1);
                panelRect.offsetMin = Vector2.zero;
                panelRect.offsetMax = Vector2.zero;
                panelRect.anchoredPosition = Vector2.zero;
            }
        }

        Debug.Log("Canvas 구조가 수정되었습니다.");
    }
}
