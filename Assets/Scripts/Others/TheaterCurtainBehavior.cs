using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

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

    private AudioManager audioManager;
    public AudioManager AudioManager
    {
        get
        {
            if (audioManager == null) audioManager = AudioManager.instance;
            return audioManager;
        }
    }

    public AudioClip openCurtainAudioClip;
    public AudioClip closeCurtainAudioClip;

    AudioSource curtainSound;

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

    public void CloseCurtain()
    {
        Animator.SetTrigger("closeCurtain");
        PlayCloseCurtainSound();
    }

    public void PlayOpenCurtainSound()
    {
        curtainSound = AudioManager.PlaySound(openCurtainAudioClip, SoundType.MetaTheater);
    }

    public void PlayCloseCurtainSound()
    {
        curtainSound = AudioManager.PlaySound(closeCurtainAudioClip, SoundType.MetaTheater);
    }

    public void StopCurtainSound(float time)
    {
        AudioManager.FadeOutSound(curtainSound, time);
    }
}
