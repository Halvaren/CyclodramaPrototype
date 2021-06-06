using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeakersController : MonoBehaviour
{
    public AudioSource leftSpeaker;
    public AudioSource rightSpeaker;

    public static SpeakersController instance;

    private void Awake()
    {
        instance = this;
    }

    public void PlaySoundOnSpeakers(AudioClip clip)
    {
        leftSpeaker.PlayOneShot(clip);
        rightSpeaker.PlayOneShot(clip);
    }
}
