using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the (physical) behavior of a cup thrown by Óliver
/// </summary>
public class ThrowableCup : MonoBehaviour
{
    //Initial force which the cup is emitted with
    public float initialForce;
    //Time until it disappears
    public float timeToDisappear;

    //Action to execute when impacts with Notan
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

    /// <summary>
    /// Adds force and torque to the cup
    /// </summary>
    /// <param name="direction"></param>
    public void Emit(Vector3 direction)
    {
        Rigidbody.AddForce(direction * initialForce);
        Rigidbody.AddTorque(Vector3.one * initialForce, ForceMode.Acceleration);
    }

    /// <summary>
    /// When time runs out, cup disappears
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    IEnumerator Disappear(float time)
    {
        yield return new WaitForSeconds(time);

        Destroy(gameObject);
    }

    /// <summary>
    /// Checks if collided object is Notan. If it is, executes gotTargetAction and starts disappearing countdown
    /// </summary>
    /// <param name="collision"></param>
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
