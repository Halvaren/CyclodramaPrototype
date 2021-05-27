using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowableCup : MonoBehaviour
{
    public float initialForce;
    public float timeToDisappear;

    public Action gotTargetAction;

    private Rigidbody m_Rigidbody;
    public Rigidbody Rigidbody
    {
        get
        {
            if (m_Rigidbody == null) m_Rigidbody = GetComponent<Rigidbody>();
            return m_Rigidbody;
        }
    }

    public void Emit(Vector3 direction)
    {
        Rigidbody.AddForce(direction * initialForce);
        Rigidbody.AddTorque(Vector3.one * initialForce, ForceMode.Acceleration);
    }

    IEnumerator Disappear(float time)
    {
        yield return new WaitForSeconds(time);

        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        NotanBehavior notan;
        if((notan = collision.gameObject.GetComponentInParent<NotanBehavior>()) != null)
        {
            gotTargetAction();

            StartCoroutine(Disappear(timeToDisappear));
        }
    }
}
