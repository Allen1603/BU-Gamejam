using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    public float changeTime = 3f;
    public GameObject comicIntro;

    private void Start()
    {
        if(AudioManager.instance != null)
            AudioManager.instance.PlayBGM(AudioManager.instance.cutsceneMusic);
    }

    void Update()
    {
        changeTime -= Time.deltaTime;

        // Start fade once time is up
        if (changeTime <= 0)
        {
            SceneManager.LoadSceneAsync(2);
        }
    }
}
