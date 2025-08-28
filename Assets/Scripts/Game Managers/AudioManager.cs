using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Manager Settings")]
    public static AudioManager Instance;
    public AudioSource musicSource;
    public AudioClip[] era1Music;
    public AudioClip[] era2Music;
    public AudioClip[] era3Music;
    [Range(0f, 1f)] public float musicVolume;
    [Range(0f, 1f)] public float sfxVolume;

    [Header("Sound Effects Settings")]
    public AudioSource sfxSource;
    public AudioClip[] SFXArray;
    public AudioClip[] ForcedSFXArray;
    public Button[] buttonsToPlaySFX;

    [Header("UI Controls")]
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Toggle musicMuteToggle;
    public Toggle sfxMuteToggle;

    private bool hasFocus = true, isMusicMuted = false, isSFXMuted = false;
    private int currentEra = 1, songIndex = 0;

    // Awake is called before the application starts
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Start()
    {
        musicSource.loop = false;
        musicSource.volume = musicVolume;
        sfxSource.volume = sfxVolume;
        PlayCurrentBGM();

        // Add Sound Listener
        for (int i = 0; i < buttonsToPlaySFX.Length; i++)
        {
            if (buttonsToPlaySFX[i] != null)
            {
                int buttonIndex = i;
                buttonsToPlaySFX[i].onClick.AddListener(() => PlayButtonSound(buttonIndex));
            }
        }

        // Add Listeners to the volume slider & toggle
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = musicVolume;
            musicVolumeSlider.onValueChanged.AddListener(OnVolumeSliderChanged);
        }

        if (musicMuteToggle != null)
        {
            musicMuteToggle.isOn = isMusicMuted;
            musicMuteToggle.onValueChanged.AddListener(OnMuteToggleChanged);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = sfxVolume;
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeSliderChanged);
        }

        if (sfxMuteToggle != null)
        {
            sfxMuteToggle.isOn = isSFXMuted;
            sfxMuteToggle.onValueChanged.AddListener(OnSFXMuteToggleChanged);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Check if the currentTurn has surpassed the current era
        // If so, play the first index of the BGM in the new era BGMs
        int newEra = GetCurrentEra();
        if (newEra != currentEra)
        {
            currentEra = newEra;
            songIndex = 0;
            PlayCurrentBGM();
        }

        // This part pauses and restarts the music on focus and off focus (e.g alt-tabbed)
        bool nowHasFocus = Application.isFocused;

        if (hasFocus && !nowHasFocus)
        {
            if (musicSource.isPlaying)
                musicSource.Pause();
        }
        else if (!hasFocus && nowHasFocus)
        {
            if (!musicSource.isPlaying && musicSource.clip != null)
                musicSource.UnPause();
        }

        hasFocus = nowHasFocus;

        // Check if the song has ended or not and make sure the application is in focus first
        if (Application.isFocused && !musicSource.isPlaying && musicSource.clip != null)
        {
            PlayNextBGM();
        }
    }

    void PlayCurrentBGM()
    {
        AudioClip[] songs = GetCurrentBGM();
        if (songs.Length == 0) return;

        musicSource.clip = songs[songIndex];
        musicSource.Play();
    }

    void PlayNextBGM()
    {
        AudioClip[] songs = GetCurrentBGM();
        if (songs.Length == 0) return;

        songIndex++;
        if (songIndex >= songs.Length)
        {
            songIndex = 0;
        }

        musicSource.clip = songs[songIndex];
        musicSource.Play();
    }

    int GetCurrentEra()
    {
        if (GameManager.Instance == null) return 1;

        int turn = GameManager.Instance.CurrentTurn;
        if (turn <= 40) return 1;
        if (turn <= 80) return 2;
        return 3;
    }

    void PlayButtonSound(int buttonIndex)
    {
        if (buttonIndex < SFXArray.Length && SFXArray[buttonIndex] != null)
        {
            sfxSource.PlayOneShot(SFXArray[buttonIndex]);
        }
    }

    public void ForcePlaceSFX(int SFXIndex)
    {
        if (SFXArray[SFXIndex] != null)
        {
            sfxSource.PlayOneShot(ForcedSFXArray[SFXIndex]);
        }
    }

    public void OnVolumeSliderChanged(float sliderValue)
    {
        musicVolume = sliderValue;
        UpdateMusicVolume();
    }

    public void OnMuteToggleChanged(bool isToggleOn)
    {
        isMusicMuted = isToggleOn;
        UpdateMusicVolume();
    }

    public void OnSFXVolumeSliderChanged(float sliderValue)
    {
        sfxVolume = sliderValue;
        UpdateSFXVolume();
    }

    public void OnSFXMuteToggleChanged(bool isToggleOn)
    {
        isSFXMuted = isToggleOn;
        UpdateSFXVolume();
    }

    void UpdateMusicVolume()
    {
        if (isMusicMuted)
        {
            musicSource.volume = 0f;
        }
        else
        {
            musicSource.volume = musicVolume;
        }
    }

    void UpdateSFXVolume()
    {
        if (isSFXMuted)
        {
            sfxSource.volume = 0f;
        }
        else
        {
            sfxSource.volume = sfxVolume;
        }
    }

    AudioClip[] GetCurrentBGM()
    {
        if (currentEra == 1) return era1Music;
        if (currentEra == 2) return era2Music;
        return era3Music;
    }
}