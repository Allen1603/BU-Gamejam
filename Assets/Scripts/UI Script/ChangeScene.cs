using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    public float changeTime = 3f;      // time before fade starts
    public float fadeDuration = 1f;    // fade speed
    public Image fadeImage;            // assign your fade panel image in inspector

    private bool isFading = false;

    void Update()
    {
        if (AudioManager.instance != null)
            AudioManager.instance.StopBGM();

        changeTime -= Time.deltaTime;

        // Start fade once time is up
        if (changeTime <= 0 && !isFading)
        {
            StartCoroutine(FadeAndChangeScene());
        }
    }

    IEnumerator FadeAndChangeScene()
    {
        isFading = true;

        float t = 0f;
        Color color = fadeImage.color;

        // Fade to black
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            color.a = Mathf.Lerp(0f, 1f, t / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        // Load next scene (index 2)
        SceneManager.LoadSceneAsync(2);
    }
}
