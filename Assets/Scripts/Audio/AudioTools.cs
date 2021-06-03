using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioTools
{
    public static IEnumerator FadeIn(MonoBehaviour behavior, AudioSource source, float fadeTime, float finalVolume = 1)
    {
        source.volume = 0;
        source.Play();

        yield return behavior.StartCoroutine(ChangeVolumeInTime(source, source.volume, finalVolume, fadeTime));
    }

    public static IEnumerator FadeOut(MonoBehaviour behavior, AudioSource source, float fadeTime, float finalVolume = 0)
    {
        float previousVolume = source.volume;
        yield return behavior.StartCoroutine(ChangeVolumeInTime(source, source.volume, finalVolume, fadeTime));

        source.Stop();
        source.volume = previousVolume;
    }

    public static IEnumerator CrossFade(MonoBehaviour behavior, AudioSource sourceIn, AudioSource sourceOut, float fadeTime, float finalVolumeIn = 1 , float finalVolumeOut = 0)
    {
        float previousVolumeOut = sourceOut.volume;

        sourceIn.volume = 0;
        sourceIn.Play();

        behavior.StartCoroutine(ChangeVolumeInTime(sourceIn, sourceIn.volume, finalVolumeIn, fadeTime));
        yield return behavior.StartCoroutine(ChangeVolumeInTime(sourceOut, sourceOut.volume, finalVolumeOut, fadeTime));

        sourceOut.Stop();
        sourceOut.volume = previousVolumeOut;
    }

    public static IEnumerator ChangeVolumeInTime(AudioSource source, float initialVolume, float finalVolume, float time)
    {
        float elapsedTime = 0.0f;

        while(elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;

            source.volume = Mathf.Lerp(initialVolume, finalVolume, elapsedTime / time);

            yield return null;
        }
        source.volume = finalVolume;
    }
}
