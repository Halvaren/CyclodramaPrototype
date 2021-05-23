using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SetDoorBehavior : DoorBehavior
{
    [HideInInspector]
    public SetTransitionTrigger transitionTrigger;

    [HideInInspector]
    public Transform doorSign;

    [HideInInspector]
    public bool signBlink = false;

    protected float blinkingTimer = 0.25f;
    protected float blinkingTime = 0.0f;

    #region SetTransitionTrigger

    [HideInInspector]
    public SetTransitionMovement setTransitionMovement;

    [HideInInspector]
    public float rotation;

    [HideInInspector]
    public float distanceBetweenSets = 25;
    [HideInInspector]
    public Vector3 offset;

    [HideInInspector]
    public GameObject nextSet;

    [HideInInspector]
    public int connectionIndex = -1;
    [HideInInspector]
    public string nextSetName;

    [HideInInspector]
    public CharacterTransitionMovement characterTransitionMovement;

    [HideInInspector]
    public bool characterXMovement;
    [HideInInspector]
    public bool characterYMovement;
    [HideInInspector]
    public bool characterZMovement;

    [HideInInspector]
    public Transform characterWaitPosition;

    [HideInInspector]
    public bool waypointsInNextTrigger;
    [HideInInspector]
    public Transform[] characterWaypoints;

    [HideInInspector]
    public bool characterWaitsUntilSetMovementIsDone;

    [HideInInspector]
    public Transform characterFinalPosition;

    #endregion

    protected override void InitializeObjBehavior()
    {
        base.InitializeObjBehavior();

        doorSign.gameObject.SetActive(false);
        transitionTrigger.door = this;
    }

    private void Update()
    {
        if(signBlink)
        {
            blinkingTime += Time.deltaTime;
            if(blinkingTime > blinkingTimer)
            {
                blinkingTime = 0.0f;
                doorSign.gameObject.SetActive(!doorSign.gameObject.activeSelf);
            }
        }
    }

    public void SetSignBlink(bool value)
    {
        if(signBlink != value)
        {
            blinkingTime = 0.0f;
            signBlink = value;
            doorSign.gameObject.SetActive(value);
        }        
    }

    public override string GetObjName()
    {
        return nextSetName;
    }

    public override void SetOpenedClosedDoor(bool value)
    {
        base.SetOpenedClosedDoor(value);

        transitionTrigger.enabled = opened;
    }
}
