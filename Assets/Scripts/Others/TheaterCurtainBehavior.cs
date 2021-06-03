using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheaterCurtainBehavior : MonoBehaviour
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

    public AudioClip openCurtainAudioClip;
    public AudioClip closeCurtainAudioClip;

    public static TheaterCurtainBehavior instance;

    private void Awake()
    {
        instance = this;
    }

    public void OpenCurtain()
    {
        Animator.SetTrigger("openCurtain");
        PlayOpenCurtainSound();
    }

    public void PlayOpenCurtainSound()
    {
        AudioSource.Stop();
        AudioSource.clip = openCurtainAudioClip;
        AudioSource.Play();
    }

    public void CloseCurtain()
    {
        Animator.SetTrigger("closeCurtain");
        PlayCloseCurtainSound();
    }

    public void PlayCloseCurtainSound()
    {
        AudioSource.Stop();
        AudioSource.clip = closeCurtainAudioClip;
        AudioSource.Play();
    }

    public void StopCurtainSound(float time)
    {
        StartCoroutine(AudioTools.FadeOut(this, AudioSource, time));
    }
}
