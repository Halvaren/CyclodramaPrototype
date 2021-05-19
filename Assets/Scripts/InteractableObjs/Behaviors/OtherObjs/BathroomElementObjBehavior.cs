using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BathroomElementObjBehavior : InteractableObjBehavior
{
    public ParticleSystem particles;
    public float duration;

    public IEnumerator UseMethod()
    {
        if (particles != null) particles.Play();

        yield return new WaitForSeconds(duration);
    }
}
