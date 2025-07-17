using UnityEngine;


public enum SoundType
{
    Running,
    Attacking,
}


[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioClip[] soundList;

    private static SoundManager instance;
    private AudioSource oneShotAudioSource;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        oneShotAudioSource = gameObject.GetComponent<AudioSource>();
    }

    public static void PlaySound(SoundType sound,float volume = 1)
    {
        instance.oneShotAudioSource.PlayOneShot(instance.soundList[(int)sound], volume);
    }

   
}
