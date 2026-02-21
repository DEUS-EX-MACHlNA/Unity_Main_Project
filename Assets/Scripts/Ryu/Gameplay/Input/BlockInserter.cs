using UnityEngine;
using TMPro;

/// <summary>
/// 블록 삽입 기능을 담당하는 클래스입니다.
/// InputField에 블록 텍스트를 삽입하고 커서 위치를 관리합니다.
/// </summary>
public class BlockInserter
{
    private TMP_InputField inputField;
    private bool needsDeselect = false;
    private int pendingCaretPos;

    /// <summary>
    /// BlockInserter 생성자
    /// </summary>
    /// <param name="inputField">InputField 컴포넌트</param>
    public BlockInserter(TMP_InputField inputField)
    {
        this.inputField = inputField;
    }

    /// <summary>
    /// InputField에 블록을 삽입합니다.
    /// </summary>
    /// <param name="blockName">블록 이름</param>
    /// <param name="showInputFieldCallback">InputField를 표시하는 콜백 (필요 시)</param>
    public void AddBlockToInput(string blockName, System.Action showInputFieldCallback = null)
    {
        if (inputField == null)
        {
            Debug.LogWarning("[BlockInserter] InputField가 null입니다.");
            return;
        }

        // ResultText 표시 중이면 InputField로 전환
        if (!inputField.gameObject.activeSelf && showInputFieldCallback != null)
        {
            showInputFieldCallback();
        }

        string blockText = $"@{blockName} ";
        int caretPos = Mathf.Clamp(inputField.caretPosition, 0, inputField.text.Length);
        inputField.text = inputField.text.Insert(caretPos, blockText);
        int newCaretPos = caretPos + blockText.Length;

        inputField.ActivateInputField();
        needsDeselect = true;
        pendingCaretPos = newCaretPos;

        Debug.Log($"[BlockInserter] 블록 추가: {blockText.Trim()}");
    }

    /// <summary>
    /// LateUpdate에서 호출하여 커서 위치를 업데이트합니다.
    /// </summary>
    public void UpdateCaretPosition()
    {
        if (needsDeselect && inputField != null)
        {
            needsDeselect = false;
            inputField.caretPosition = pendingCaretPos;
            inputField.selectionAnchorPosition = pendingCaretPos;
            inputField.selectionFocusPosition = pendingCaretPos;
        }
    }
}

