using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using RotaryHeart.Lib.SerializableDictionary;

public enum SoundType
{
    General, Music, Ambience, SFX, Character, Footstep, MetaTheater, Set, UI 
}

public class AudioManager : MonoBehaviour
{
    Queue<AudioSource> audioSourcePool;
    public GameObject poolObject;
    public int poolSize;
    public MixerGroupDictionary mixerGroupDictionary;

    public static AudioManager instance;

    private void Awake()
    {
        instance = this;
    }

    public IEnumerator FillPool()
    {
        audioSourcePool = new Queue<AudioSource>();

        for(int i = 0; i < poolSize; i++)
        {
            AudioSource audioSource = Instantiate(poolObject, transform.position, Quaternion.identity, transform).GetComponent<AudioSource>();

            audioSourcePool.Enqueue(audioSource);
            audioSource.gameObject.SetActive(false);

            yield return null;
        }
    }

    public AudioSource PlaySound(AudioClip clip, SoundType type, bool loop = false)
    {
        AudioSource audioSource = audioSourcePool.Dequeue();
        audioSource.gameObject.SetActive(true);

        audioSource.clip = clip;
        audioSource.outputAudioMixerGroup = mixerGroupDictionary[type];
        audioSource.loop = loop;

        StartCoroutine(PlaySoundCoroutine(audioSource));

        return audioSource;
    }

    public AudioSource PlaySound(AudioClip clip, SoundType type, float fadeTime, float finalVolume = 1, bool loop = false)
    {
        AudioSource audioSource = audioSourcePool.Dequeue();
        audioSource.gameObject.SetActive(true);

        audioSource.clip = clip;
        audioSource.outputAudioMixerGroup = mixerGroupDictionary[type];
        audioSource.loop = loop;

        StartCoroutine(FadeIn(audioSource, fadeTime, finalVolume));

        return audioSource;
    }

    public void FadeOutSound(AudioSource audioSource, float fadeTime, float finalVolume = 0)
    {
        if (audioSource == null) return;
        StartCoroutine(FadeOut(audioSource, fadeTime, finalVolume));
    }

    IEnumerator PlaySoundCoroutine(AudioSource source)
    {
        source.Play();

        while (source.isPlaying)
            yield return null;

        source.gameObject.SetActive(false);

        audioSourcePool.Enqueue(source);
    }

    IEnumerator FadeIn(AudioSource source, float fadeTime, float finalVolume = 1)
    {
        source.volume = 0;
        StartCoroutine(PlaySoundCoroutine(source));

        yield return StartCoroutine(ChangeVolumeInTime(source, source.volume, finalVolume, fadeTime));
    }

    IEnumerator FadeOut(AudioSource source, float fadeTime, float finalVolume = 0)
    {
        yield return StartCoroutine(ChangeVolumeInTime(source, source.volume, finalVolume, fadeTime));

        source.Stop();
    }

    IEnumerator ChangeVolumeInTime(AudioSource source, float initialVolume, float finalVolume, float time)
    {
        float elapsedTime = 0.0f;

        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;

            source.volume = Mathf.Lerp(initialVolume, finalVolume, elapsedTime / time);

            yield return null;
        }
        source.volume = finalVolume;
    }
}

[System.Serializable]
public class MixerGroupDictionary : SerializableDictionaryBase<SoundType, AudioMixerGroup>
{

}
