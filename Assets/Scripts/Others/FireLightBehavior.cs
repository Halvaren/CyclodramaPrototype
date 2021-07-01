using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages fire light from atrezzo workshop: scales it and descales it
/// </summary>
public class FireLightBehavior : MonoBehaviour
{
    public float maxScale;
    public float minScale;

    public float scalingSpeed;
    bool scaling;

    private Light fireLight;
    public Light FireLight
    {
        get
        {
            if (fireLight == null) fireLight = GetComponent<Light>();
            return fireLight;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(scaling)
        {
            FireLight.range += scalingSpeed * Time.deltaTime * Random.value;

            if (FireLight.range > maxScale) scaling = false;
        }
        else
        {
            FireLight.range -= scalingSpeed * Time.deltaTime * Random.value;

            if (FireLight.range < minScale) scaling = true;
        }
    }
}
