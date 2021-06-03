using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheaterGearsBehavior : MonoBehaviour
{
    private Animator animator;
    public Animator Animator
    {
        get
        {
            if (animator == null) animator = GetComponent<Animator>();
            return animator;
        }
    }

    private AudioSource audioSource;
    public AudioSource AudioSource
    {
        get
        {
            if (audioSource == null) audioSource = GetComponent<AudioSource>();
            return audioSource;
        }
    }

    public List<AudioClip> gearsAudioClips;

    public static TheaterGearsBehavior instance;

    private void Awake()
    {
        instance = this;
    }

    public void TurnOnOffGears(bool on)
    {
        Animator.SetBool("turningGears", on);
        if (on)
            PlayGearsSound();
    }

    public void PlayGearsSound()
    {
        AudioSource.Stop();

        int randNum = Random.Range(0, gearsAudioClips.Count);
        AudioSource.clip = gearsAudioClips[randNum];

        AudioSource.Play();
    }

    public void StopGearsSound(float time)
    {
        StartCoroutine(AudioTools.FadeOut(this, AudioSource, time));
    }
}
