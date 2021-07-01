using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Manages metatheater gears behavior
/// </summary>
public class TheaterGearsBehavior : MonoBehaviour
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

    public AudioClip backgroundClip;
    public AudioClip leftClip;
    public AudioClip rightClip;
    public AudioClip backClip;
    public AudioClip frontClip;
    public AudioClip rotationClip;
    public AudioClip endClip;

    AudioSource backgroundSound;

    public static TheaterGearsBehavior instance;

    #endregion

    #region Methods

    private void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// Turns on or off the gears. If it turns on and was already on, the current background sound stops to avoid double playing
    /// </summary>
    /// <param name="on"></param>
    public void TurnOnOffGears(bool on)
    {
        Animator.SetBool("turningGears", on);

        if (on)
        {
            if(backgroundSound != null)
            {
                backgroundSound.Stop();
                backgroundSound = null;
            }
            PlayGearsBackgroundSound();
        }
    }

    /// <summary>
    /// Plays gears background soung
    /// </summary>
    public void PlayGearsBackgroundSound()
    {
        backgroundSound = AudioManager.PlaySound(backgroundClip, SoundType.MetaTheater, true);
    }

    /// <summary>
    /// Plays main gears sound, depending on the set transition movement
    /// </summary>
    /// <param name="movement"></param>
    public void PlayGearsMovementSound(SetMovement movement)
    {
        AudioClip clip = null;
        switch(movement)
        {
            case SetMovement.Left:
                clip = leftClip;
                break;
            case SetMovement.Right:
                clip = rightClip;
                break;
            case SetMovement.Towards:
                clip = frontClip;
                break;
            case SetMovement.Backwards:
                clip = backClip;
                break;
            case SetMovement.Up:
                clip = frontClip;
                break;
            case SetMovement.Down:
                clip = backClip;
                break;
        }

        AudioManager.PlaySound(clip, SoundType.MetaTheater);
    }

    /// <summary>
    /// Plays set rotation sound
    /// </summary>
    public void PlayGearsRotationSound()
    {
        AudioManager.PlaySound(rotationClip, SoundType.MetaTheater);
    }

    /// <summary>
    /// Fades out background sound and plays the end sound
    /// </summary>
    /// <param name="time"></param>
    public void StopGearsSound(float time)
    {
        AudioManager.FadeOutSound(backgroundSound, time);
        AudioManager.PlaySound(endClip, SoundType.MetaTheater, time);
        backgroundSound = null;
    }

    #endregion
}
