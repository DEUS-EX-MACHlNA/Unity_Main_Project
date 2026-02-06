using UnityEngine;

public class FloatingWorld : MonoBehaviour
{
    public float amplitude = 0.02f;      // 이동 폭 (유닛)
    public float speed = 0.5f;           // 느리게
    public float rotationAmplitude = 0.6f; // 도(deg)
    public float rotationSpeed = 0.3f;

    Vector3 startPos;

    void Awake()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float t = Time.time;

        float y = Mathf.Sin(t * speed) * amplitude;
        transform.position = startPos + Vector3.up * y;

        float rot = Mathf.Sin(t * rotationSpeed) * rotationAmplitude;
        transform.rotation = Quaternion.Euler(0f, 0f, rot);
    }
}