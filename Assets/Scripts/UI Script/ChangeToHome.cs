using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeToHome : MonoBehaviour
{
    public float changeTime = 3f;

    private void Start()
    {
        if (AudioManager.instance != null)
            AudioManager.instance.PlayBGM(AudioManager.instance.cutsceneMusic);
    }

    void Update()
    {
        changeTime -= Time.deltaTime;

        // Start fade once time is up
        if (changeTime <= 0)
        {
            SceneManager.LoadSceneAsync(0);
        }
    }
}
