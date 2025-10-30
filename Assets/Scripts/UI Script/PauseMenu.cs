using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    private void Start()
    {
        if(AudioManager.instance != null)
            AudioManager.instance.PlayBGM(AudioManager.instance.ingameMusic);
    }
    public void Pause()
    {
        Time.timeScale = 0;
    }

    public void Home()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
    }
    public void Restart()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(2);
    }
    public void Resume()
    {
        Time.timeScale = 1;
    }
}
