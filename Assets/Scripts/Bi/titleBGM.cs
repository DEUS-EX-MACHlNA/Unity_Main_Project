using UnityEngine;

public class TitleBGM : MonoBehaviour
{
    public AudioClip bgmClip;

    void Awake()
    {
        AudioSource audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = bgmClip;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
    }

    void Start()
    {
        AudioSource aus = GetComponent<AudioSource>();
        Debug.Log($"clip: {aus.clip}, volume: {aus.volume}");
        aus.Play();
        Debug.Log($"isPlaying: {aus.isPlaying}");
    }
}