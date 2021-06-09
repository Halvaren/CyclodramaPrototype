using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BathroomElementObjBehavior : InteractableObjBehavior
{
    public ParticleSystem particles;
    public AudioClip clip;

    AudioSource source;
    Coroutine stopParticlesCoroutine;

    public IEnumerator UseMethod()
    {
        if (particles != null)
        {
            particles.Play();
            if (stopParticlesCoroutine != null) StopCoroutine(stopParticlesCoroutine);
            stopParticlesCoroutine = StartCoroutine(StopPS(clip.length));
        }

        if (source != null)
        {
            source.Stop();
            source = null;
        }
        source = AudioManager.PlaySound(clip, SoundType.Set);

        yield return null;
    }

    IEnumerator StopPS(float time)
    {
        yield return new WaitForSeconds(time);
        particles.Stop();

        stopParticlesCoroutine = null;
    }
}
