using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SetTransitionTrigger : MonoBehaviour
{
    public SetTransitionMovement setTransitionMovement;

    public float rotation;

    public float distanceBetweenSets = 25;
    public Vector3 offset;

    public GameObject currentSet;
    public GameObject nextSet;

    public int connectionIndex = -1;
    public string nextSetName;

    public CharacterTransitionMovement characterTransitionMovement;

    public bool characterXMovement;
    public bool characterYMovement;
    public bool characterZMovement;

    public Transform characterWaitPosition;

    public bool waypointsInNextTrigger;
    public Transform[] characterWaypoints;

    public bool characterWaitsUntilSetMovementIsDone;

    public Transform characterFinalPosition;

    private void OnTriggerEnter(Collider other)
    {
        PCController pcController = other.GetComponent<PCController>();
        //SetTransitionSystem.instance.ExecuteSetTransition(this, pcController);
    }
}
