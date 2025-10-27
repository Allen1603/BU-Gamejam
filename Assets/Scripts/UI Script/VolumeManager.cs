using UnityEngine;
using UnityEngine.UI;

public class VolumeManager : MonoBehaviour
{
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider sfxSlider;

    private void Awake()
    {
        if (!PlayerPrefs.HasKey("musicVolume") || !PlayerPrefs.HasKey("sfxVolume"))
        {
            PlayerPrefs.SetFloat("musicVolume", 1);
            PlayerPrefs.SetFloat("sfxVolume", 1);
        }

        LoadVolume();
    }

    public void SetMusicVolume()
    {
        AudioManager.instance.sourceMusic.volume = musicSlider.value;

        PlayerPrefs.SetFloat("musicVolume", musicSlider.value);
    }

    public void SetSFXVolume()
    {
        AudioManager.instance.sfxSource.volume = sfxSlider.value;

        PlayerPrefs.SetFloat("sfxVolume", sfxSlider.value);
    }

    private void LoadVolume()
    {
        float musicVolume = PlayerPrefs.GetFloat("musicVolume");
        float sfxVolume = PlayerPrefs.GetFloat("sfxVolume");

        musicSlider.value = musicVolume;
        sfxSlider.value = sfxVolume;

        if (AudioManager.instance != null)
        {
            AudioManager.instance.sourceMusic.volume = musicVolume;
            AudioManager.instance.sfxSource.volume = sfxVolume;
        }
    }
}
