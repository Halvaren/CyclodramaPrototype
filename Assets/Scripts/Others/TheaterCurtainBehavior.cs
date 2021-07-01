using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Manages metatheater curtains behavior
/// </summary>
public class TheaterCurtainBehavior : MonoBehaviour
{
    #region Variables

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

    #endregion

    #region Method

    private void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// Opens curtain (animation and sound)
    /// </summary>
    public void OpenCurtain()
    {
        Animator.SetTrigger("openCurtain");
        PlayOpenCurtainSound();
    }

    /// <summary>
    /// Closes curtain (animation and sound)
    /// </summary>
    public void CloseCurtain()
    {
        Animator.SetTrigger("closeCurtain");
        PlayCloseCurtainSound();
    }

    /// <summary>
    /// Plays opening curtain sound
    /// </summary>
    public void PlayOpenCurtainSound()
    {
        curtainSound = AudioManager.PlaySound(openCurtainAudioClip, SoundType.MetaTheater);
    }

    /// <summary>
    /// Plays closing curtain sound
    /// </summary>
    public void PlayCloseCurtainSound()
    {
        curtainSound = AudioManager.PlaySound(closeCurtainAudioClip, SoundType.MetaTheater);
    }

    /// <summary>
    /// Fades curtain sound out
    /// </summary>
    /// <param name="time"></param>
    public void StopCurtainSound(float time)
    {
        AudioManager.FadeOutSound(curtainSound, time);
    }

    #endregion
}
