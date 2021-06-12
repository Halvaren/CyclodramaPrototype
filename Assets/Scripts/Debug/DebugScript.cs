using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugScript : MonoBehaviour
{
    public GameObject srtnr;

    private void Start()
    {
        StartCoroutine(Coroutine());
    }

    IEnumerator Coroutine()
    {
        while(true)
        {
            yield return new WaitForSeconds(0.1f);
            Instantiate(srtnr);
        }
    }
}
