using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SetTransitionTrigger : MonoBehaviour
{
    public SetDoorBehavior door;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PCController>() != null)
        {
            PCController pcController = other.GetComponent<PCController>();
            SetTransitionSystem.instance.ExecuteSetTransition(door, pcController);
        }
    }
}
