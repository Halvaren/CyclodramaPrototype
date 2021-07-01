using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using RotaryHeart.Lib.SerializableDictionary;

public enum SoundType
{
    General, Music, Ambience, SFX, Character, Footstep, MetaTheater, Set, UI, BackgroundMusic, ForegroundMusic
}

/// <summary>
/// Manages the audio source pool. Creates, plays, fades and destroys audio sources
/// </summary>
public class AudioManager : MonoBehaviour
{
    Queue<AudioSource> audioSourcePool;
    public GameObject poolObject;
    public int poolSize;
    public MixerGroupDictionary mixerGroupDictionary;

    public AudioClip initialMusicIntro;
    public AudioClip initialMusicLoop;
    AudioSource initialMusic;

    public static AudioManager instance;

    private void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// Initializes audio source pool
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Plays initial music in loop
    /// </summary>
    public void PlayMenuMusic()
    {
        initialMusic = PlaySoundIntroAndLoop(initialMusicIntro, initialMusicLoop, SoundType.BackgroundMusic);
    }

    /// <summary>
    /// Stops initial music if it's playing
    /// </summary>
    public void StopMenuMusic()
    {
        if(initialMusic != null)
        {
            initialMusic.Stop();
            initialMusic.gameObject.SetActive(false);

            audioSourcePool.Enqueue(initialMusic);

            initialMusic = null;
        }
    }

    /// <summary>
    /// Fades out initial music if it's playing
    /// </summary>
    /// <param name="fadeTime"></param>
    /// <returns></returns>
    public IEnumerator StopMenuMusic(float fadeTime)
    {
        if(initialMusic != null)
        {
            yield return StartCoroutine(FadeOut(initialMusic, fadeTime));
            initialMusic.gameObject.SetActive(false);

            audioSourcePool.Enqueue(initialMusic);

            initialMusic = null;
        }
    }

    /// <summary>
    /// Plays clip, assigning to the audioSource the correct audioMixer according to type
    /// </summary>
    /// <param name="clip"></param>
    /// <param name="type"></param>
    /// <param name="loop"></param>
    /// <returns></returns>
    public AudioSource PlaySound(AudioClip clip, SoundType type, bool loop = false)
    {
        AudioSource audioSource = audioSourcePool.Dequeue();
        audioSource.gameObject.SetActive(true);

        audioSource.volume = 1;
        audioSource.clip = clip;
        audioSource.outputAudioMixerGroup = mixerGroupDictionary[type];
        audioSource.loop = loop;

        StartCoroutine(PlaySoundCoroutine(audioSource));

        return audioSource;
    }

    /// <summary>
    /// Fades in clip, assigning to the audioSource the correct audioMixer according to type
    /// </summary>
    /// <param name="clip"></param>
    /// <param name="type"></param>
    /// <param name="fadeTime"></param>
    /// <param name="finalVolume"></param>
    /// <param name="loop"></param>
    /// <returns></returns>
    public AudioSource PlaySound(AudioClip clip, SoundType type, float fadeTime, float finalVolume = 1, bool loop = false)
    {
        AudioSource audioSource = audioSourcePool.Dequeue();
        audioSource.gameObject.SetActive(true);

        audioSource.volume = 1;
        audioSource.clip = clip;
        audioSource.outputAudioMixerGroup = mixerGroupDictionary[type];
        audioSource.loop = loop;

        StartCoroutine(FadeIn(audioSource, fadeTime, finalVolume));

        return audioSource;
    }

    /// <summary>
    /// Plays introClip until it is finished and then plays loopClip in loop
    /// </summary>
    /// <param name="introClip"></param>
    /// <param name="loopClip"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public AudioSource PlaySoundIntroAndLoop(AudioClip introClip, AudioClip loopClip, SoundType type)
    {
        AudioSource audioSource = audioSourcePool.Dequeue();
        audioSource.gameObject.SetActive(true);

        audioSource.volume = 1;
        audioSource.clip = introClip;
        audioSource.outputAudioMixerGroup = mixerGroupDictionary[type];
        audioSource.loop = false;

        StartCoroutine(PlaySoundIntroAndLoopCoroutine(audioSource, loopClip));

        return audioSource;
    }

    /// <summary>
    /// Fades out audioSource in fadeTime seconds
    /// </summary>
    /// <param name="audioSource"></param>
    /// <param name="fadeTime"></param>
    /// <param name="finalVolume"></param>
    public void FadeOutSound(AudioSource audioSource, float fadeTime, float finalVolume = 0)
    {
        if (audioSource == null) return;
        StartCoroutine(FadeOut(audioSource, fadeTime, finalVolume));
    }

    /// <summary>
    /// Plays source and, when it's finished, deactivates it and enqueues it again to the pool queue
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    IEnumerator PlaySoundCoroutine(AudioSource source)
    {
        source.Play();

        while (source.isPlaying)
            yield return null;

        source.gameObject.SetActive(false);

        audioSourcePool.Enqueue(source);
    }

    /// <summary>
    /// Plays current clip in source and, when it's finished, replaces current clip with loopClip and, activates loop and plays source again
    /// </summary>
    /// <param name="source"></param>
    /// <param name="loopClip"></param>
    /// <returns></returns>
    IEnumerator PlaySoundIntroAndLoopCoroutine(AudioSource source, AudioClip loopClip)
    {
        source.Play();

        while (source.isPlaying)
            yield return null;

        source.loop = true;
        source.clip = loopClip;

        StartCoroutine(PlaySoundCoroutine(source));
    }

    /// <summary>
    /// Fades source in
    /// </summary>
    /// <param name="source"></param>
    /// <param name="fadeTime"></param>
    /// <param name="finalVolume"></param>
    /// <returns></returns>
    IEnumerator FadeIn(AudioSource source, float fadeTime, float finalVolume = 1)
    {
        source.volume = 0;
        StartCoroutine(PlaySoundCoroutine(source));

        yield return StartCoroutine(ChangeVolumeInTime(source, source.volume, finalVolume, fadeTime));
    }

    /// <summary>
    /// Fades source out
    /// </summary>
    /// <param name="source"></param>
    /// <param name="fadeTime"></param>
    /// <param name="finalVolume"></param>
    /// <returns></returns>
    IEnumerator FadeOut(AudioSource source, float fadeTime, float finalVolume = 0)
    {
        yield return StartCoroutine(ChangeVolumeInTime(source, source.volume, finalVolume, fadeTime));

        if(finalVolume == 0)
            source.Stop();
    }

    /// <summary>
    /// Fades source's volume between two values in time
    /// </summary>
    /// <param name="source"></param>
    /// <param name="initialVolume"></param>
    /// <param name="finalVolume"></param>
    /// <param name="time"></param>
    /// <returns></returns>
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


/// <summary>
/// Serializable dictionary (visible and editable in Inspector) of pairs of SoundType and AudioMixerGroup
/// </summary>
[System.Serializable]
public class MixerGroupDictionary : SerializableDictionaryBase<SoundType, AudioMixerGroup>
{

}
