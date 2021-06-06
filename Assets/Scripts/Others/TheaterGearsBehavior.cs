using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

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

    private void Awake()
    {
        instance = this;
    }

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

    public void PlayGearsBackgroundSound()
    {
        backgroundSound = AudioManager.PlaySound(backgroundClip, SoundType.MetaTheater, true);
    }

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

    public void PlayGearsRotationSound()
    {
        AudioManager.PlaySound(rotationClip, SoundType.MetaTheater);
    }

    public void StopGearsSound(float time)
    {
        AudioManager.FadeOutSound(backgroundSound, time);
        AudioManager.PlaySound(endClip, SoundType.MetaTheater, time);
        backgroundSound = null;
    }
}
