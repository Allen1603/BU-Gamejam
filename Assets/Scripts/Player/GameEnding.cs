using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEnding : MonoBehaviour
{
    private void Start()
    {
        if (AudioManager.instance != null)
            AudioManager.instance.PlayBGM(AudioManager.instance.ingameMusic);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadSceneAsync(3);
        }
    }
}
