using UnityEngine;
using UnityEngine.UI;

public class LoadingSpinner : MonoBehaviour
{
    public static LoadingSpinner Instance;

    public GameObject spinnerObject;
    public float rotateSpeed = 470f;

    private bool isLoading = false;

    void Start()
    {
        if (spinnerObject != null)
            spinnerObject.SetActive(false);
    }
    void Awake()
    {
        // 싱글톤
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Update()
    {
        if (isLoading && spinnerObject != null)
        {
            spinnerObject.GetComponent<RectTransform>().Rotate(0f, 0f, -rotateSpeed * Time.deltaTime);
        }
    }

    public void ShowLoading()
    {
        isLoading = true;
        spinnerObject.SetActive(true);
    }

    public void HideLoading()
    {
        isLoading = false;
        spinnerObject.SetActive(false);
    }
}