using System.Collections;
using UnityEngine;
using TMPro;

public class DialogueSystem : MonoBehaviour
{
    [TextArea(3, 10)]
    public string[] paragraphs;   // 소설 문단들
    public TextMeshProUGUI textDisplay; // 글자가 나타날 TMP 객체
    public GameObject nextIcon;   // [추가된 부분] 다음 화살표 아이콘
    public float typingSpeed = 0.05f;

    private int index = 0;
    private bool isTyping = false;

    void Start()
    {
        // 시작 시 초기화
        textDisplay.text = "";
        if (nextIcon != null) nextIcon.SetActive(false);
        
        DisplayNextParagraph();
    }

    void Update()
    {
        // 클릭 시 작동
        if (Input.GetMouseButtonDown(0))
        {
            if (isTyping)
            {
                // 타이핑 중일 때 클릭하면 문장 즉시 완성
                StopAllCoroutines();
                textDisplay.text = paragraphs[index - 1];
                isTyping = false;
                if (nextIcon != null) nextIcon.SetActive(true);
            }
            else
            {
                // 문장이 완성된 상태에서 클릭하면 다음 문단으로
                DisplayNextParagraph();
            }
        }
    }

    void DisplayNextParagraph()
    {
        if (index < paragraphs.Length)
        {
            if (nextIcon != null) nextIcon.SetActive(false);
            StartCoroutine(TypeParagraph(paragraphs[index]));
            index++;
        }
        else
        {
            textDisplay.text = "이야기가 끝났습니다.";
            if (nextIcon != null) nextIcon.SetActive(false);
        }
    }

    // [이 부분이 중복되었던 함수입니다]
    IEnumerator TypeParagraph(string line)
    {
        isTyping = true;
        textDisplay.text = "";

        foreach (char letter in line.ToCharArray())
        {
            textDisplay.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        if (nextIcon != null) nextIcon.SetActive(true);
    }
}


