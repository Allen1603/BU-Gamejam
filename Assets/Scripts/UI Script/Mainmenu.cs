using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Mainmenu : MonoBehaviour
{
    private void Start()
    {
        AudioManager.instance.PlayBGM(AudioManager.instance.mainmenuMusic);
        Time.timeScale = 1;
    }
    public void Play(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }


    public void Quit()
    {
        Application.Quit();
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
        SceneManager.LoadScene(1);
    }
    public void Unplay()
    {
        Time.timeScale = 1;
    }
}
