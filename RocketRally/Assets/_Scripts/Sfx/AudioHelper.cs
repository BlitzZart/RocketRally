using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioHelper
{
    public static void PlaySound(AudioSource src, AudioClip clip, float volume = 1.0f, float rndLow = 1.0f, float rndHigh = 1.0f)
    {
        if (rndLow != 1.0f || rndLow != rndHigh)
        {
            src.pitch = Random.Range(rndLow, rndHigh);
        }
        src.clip = clip;
        src.volume = volume;
        src.Play();
    }

    public static void PlaySoundOneShot(AudioSource src, AudioClip clip, float volume = 1.0f, float rndLow = 1.0f, float rndHigh = 1.0f)
    {
        if (rndLow != 1.0f || rndLow != rndHigh)
        {
            src.pitch = Random.Range(rndLow, rndHigh);
        }

        src.PlayOneShot(clip, volume);
    }
}
