using UnityEngine;
using UnityEngine.UI;

public class SoundToggle : MonoBehaviour
{
    private bool isSoundOn = true;
    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
        UpdateButtonVisual();
    }

    public void OnSoundButtonClicked()
    {
        isSoundOn = !isSoundOn;
        AudioListener.volume = isSoundOn ? 1f : 0f;
        UpdateButtonVisual();
    }

    private void UpdateButtonVisual()
    {
        if (button != null)
        {
            ColorBlock cb = button.colors;
            cb.normalColor = isSoundOn ? Color.white : new Color(1f, 1f, 1f, 0.4f);
            button.colors = cb;
        }
    }
}