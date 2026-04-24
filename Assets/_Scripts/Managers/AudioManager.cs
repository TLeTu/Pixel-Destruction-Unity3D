using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    [Header("Audio Clips")]
    public AudioClip musicClip;
    public AudioClip levelWinClip;
    [Header("Audio Settings")]
    [SerializeField][Range(0f, 1f)] private float _musicVolume = 0.7f;
    [SerializeField][Range(0f, 1f)] private float _sfxVolume = 0.8f;
    [SerializeField] private bool _muteMusic = false;
    [SerializeField] private bool _muteSFX = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        if (musicClip != null)
        {
            PlayMusic(musicClip);
        }
    }

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (musicSource.clip == clip) return;

        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
        musicSource.clip = null;
    }

    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }
    public void PlayLevelWinSFX()
    {
        if (levelWinClip != null)
        {
            PlaySFX(levelWinClip);
        }
    }
}