using UnityEngine;

public class SoundThunder : MonoBehaviour
{
    public AudioSource audioSource;
    public float delay = 5f; 

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
