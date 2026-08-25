using UnityEngine;

public sealed class AudioManager : MonoBehaviour
{
    private const string SoundEnabledKey =
        "RoyalSmash.SoundEnabled";

    private const string MusicEnabledKey =
        "RoyalSmash.MusicEnabled";


    public static AudioManager Instance { get; private set; }


    [Header("Audio Sources")]
    [Tooltip(
        "Background music ke liye dedicated AudioSource."
    )]
    [SerializeField] private AudioSource musicSource;

    [Tooltip(
        "Gameplay/UI one-shot sounds ke liye dedicated AudioSource."
    )]
    [SerializeField] private AudioSource sfxSource;


    [Header("Music")]
    [SerializeField] private AudioClip backgroundMusic;

    [SerializeField] private bool playMusicOnStart = true;

    [SerializeField] private bool loopMusic = true;


    [Header("Behaviour")]
    [SerializeField] private bool dontDestroyOnLoad = true;


    private bool soundEnabled = true;
    private bool musicEnabled = true;


    public bool IsSoundEnabled =>
        soundEnabled;

    public bool IsMusicEnabled =>
        musicEnabled;


    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (dontDestroyOnLoad)
        {
            DontDestroyOnLoad(gameObject);
        }

        LoadSettings();
        ApplySoundState();
        ApplyMusicState();
    }


    private void Start()
    {
        if (musicSource != null)
        {
            musicSource.loop = loopMusic;
        }

        if (playMusicOnStart)
        {
            PlayBackgroundMusic();
        }
    }


    private void LoadSettings()
    {
        soundEnabled =
            PlayerPrefs.GetInt(
                SoundEnabledKey,
                1
            ) == 1;

        musicEnabled =
            PlayerPrefs.GetInt(
                MusicEnabledKey,
                1
            ) == 1;
    }


    public void SetSoundEnabled(
        bool isEnabled)
    {
        soundEnabled = isEnabled;

        PlayerPrefs.SetInt(
            SoundEnabledKey,
            soundEnabled ? 1 : 0
        );

        PlayerPrefs.Save();

        ApplySoundState();
    }


    public void SetMusicEnabled(
        bool isEnabled)
    {
        musicEnabled = isEnabled;

        PlayerPrefs.SetInt(
            MusicEnabledKey,
            musicEnabled ? 1 : 0
        );

        PlayerPrefs.Save();

        ApplyMusicState();

        if (musicEnabled &&
            playMusicOnStart &&
            musicSource != null &&
            !musicSource.isPlaying)
        {
            PlayBackgroundMusic();
        }
    }


    private void ApplySoundState()
    {
        if (sfxSource != null)
        {
            sfxSource.mute =
                !soundEnabled;
        }
    }


    private void ApplyMusicState()
    {
        if (musicSource != null)
        {
            musicSource.mute =
                !musicEnabled;
        }
    }


    public void PlayBackgroundMusic()
    {
        if (musicSource == null)
        {
            return;
        }

        if (backgroundMusic != null &&
            musicSource.clip != backgroundMusic)
        {
            musicSource.clip =
                backgroundMusic;
        }

        musicSource.loop =
            loopMusic;

        musicSource.mute =
            !musicEnabled;

        if (musicSource.clip != null &&
            !musicSource.isPlaying)
        {
            musicSource.Play();
        }
    }


    public void StopBackgroundMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }


    public void PlaySFX(
        AudioClip clip)
    {
        PlaySFX(
            clip,
            1f
        );
    }


    public void PlaySFX(
        AudioClip clip,
        float volumeScale)
    {
        if (!soundEnabled ||
            sfxSource == null ||
            clip == null)
        {
            return;
        }

        sfxSource.PlayOneShot(
            clip,
            Mathf.Clamp01(volumeScale)
        );
    }


    public void StopAllSFX()
    {
        if (sfxSource != null)
        {
            sfxSource.Stop();
        }
    }
}
