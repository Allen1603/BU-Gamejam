using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BtnSFX : MonoBehaviour
{
    public AudioSource myFx;
    public AudioClip hoverSfx;
    public AudioClip clickSfx;

    public void HoverSound()
    {
        myFx.PlayOneShot(hoverSfx);
    }
    
    public void ClickSound()
    {
        myFx.PlayOneShot(clickSfx);
    }
}
