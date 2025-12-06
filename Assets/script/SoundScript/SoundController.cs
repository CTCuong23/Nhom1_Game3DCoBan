using UnityEngine;

public class PlaySoundLoop : MonoBehaviour
{
    public AudioSource audioSource;
    public float delay = 36f; 

    void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        InvokeRepeating("PlaySound", 0f, delay);
    }

    void PlaySound()
    {
        audioSource.Play();
    }
}
