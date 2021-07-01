using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the behavior of the metatheater speakers. Speaker sources has 3D audio settings activated
/// </summary>
public class SpeakersController : MonoBehaviour
{
    public AudioSource leftSpeaker;
    public AudioSource rightSpeaker;

    public static SpeakersController instance;

    private void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// Plays the clip just one time and ignoring previous plays in both speakers
    /// </summary>
    /// <param name="clip"></param>
    public void PlaySoundOnSpeakers(AudioClip clip)
    {
        leftSpeaker.PlayOneShot(clip);
        rightSpeaker.PlayOneShot(clip);
    }
}
