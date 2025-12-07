using UnityEngine;

public class LoopWithDelay : MonoBehaviour
{
    public AudioSource audioSource;
    public float delay = 15f;

    void Start()
    {
        InvokeRepeating(nameof(PlayAlarm), 0f, delay);
    }

    void PlayAlarm()
    {
        audioSource.Play();
    }
}
