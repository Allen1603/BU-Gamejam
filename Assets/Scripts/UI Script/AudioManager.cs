using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("----Audio Source-----")]
    public AudioSource sourceMusic;
    public AudioSource sfxSource;

    [Header("----Audio Clip-----")]
    public AudioClip mainmenuMusic;
    public AudioClip ingameMusic;
    public AudioClip cutsceneMusic;
    public AudioClip grabEnemy;
    public AudioClip enemyDisolve;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            float musicVol = PlayerPrefs.GetFloat("musicVolume", 1f);
            float sfxVol = PlayerPrefs.GetFloat("sfxVolume", 1f);

            sourceMusic.volume = musicVol;
            sfxSource.volume = sfxVol;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StopBGM(AudioClip clip)
    {
        sourceMusic.Stop();
    }

    public void Playsfx(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }

    public void PlayBGM(AudioClip clip)
    {
        sourceMusic.clip = clip;
        sourceMusic.Play();
    }

    public void MuteToggle(bool muted)
    {
        if (muted)
        {
            sourceMusic.mute = true;
            sfxSource.mute = true;
        }
        else
        {
            sourceMusic.mute = false;
            sfxSource.mute = false;

            // Restore saved volumes after unmuting
            sourceMusic.volume = PlayerPrefs.GetFloat("musicVolume", 1f);
            sfxSource.volume = PlayerPrefs.GetFloat("sfxVolume", 1f);
        }
    }
}
