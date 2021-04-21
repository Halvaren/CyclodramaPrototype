using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DoorBehavior : InteractableObjBehavior
{
    public bool opened = false;
    bool openningClosing = false;

    public float[] closedAngles;
    public float[] openedAngles;

    public Transform[] doorMeshes;
    public Transform doorSign;
    public Collider doorCollider;

    #region SetTransitionTrigger

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

    #endregion

    private void OnEnable()
    {
        opened = false;
    }

    public void OpenDoor()
    {
        if (!opened && !openningClosing)
        {
            for(int i = 0; i < doorMeshes.Length; i++)
            {
                StartCoroutine(OpenCloseDoorCoroutine(doorMeshes[i], closedAngles[i], openedAngles[i], 0.5f, i == 0));
            }
        }
            
    }

    public void CloseDoor()
    {
        if (opened && !openningClosing)
        {
            for(int i = 0; i < doorMeshes.Length; i++)
            {
                StartCoroutine(OpenCloseDoorCoroutine(doorMeshes[i], openedAngles[i], closedAngles[i], 0.5f, i == 0));
            }
        }
    }

    IEnumerator OpenCloseDoorCoroutine(Transform doorMesh, float initialAngle, float finalAngle, float time, bool first = false)
    {
        if(first) openningClosing = true;

        float currentAngle = initialAngle;

        float elapsedTime = 0.0f;

        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;

            currentAngle = Mathf.Lerp(initialAngle, finalAngle, elapsedTime / time);

            doorMesh.eulerAngles = new Vector3(doorMesh.eulerAngles.x, currentAngle, doorMesh.eulerAngles.z);

            yield return null;
        }
        doorMesh.eulerAngles = new Vector3(doorMesh.eulerAngles.x, finalAngle, doorMesh.eulerAngles.z);

        if(first)
        {
            opened = !opened;
            openningClosing = false;

            if (doorCollider) doorCollider.enabled = !opened;

            currentSet.GetComponent<NavMeshSurface>().BuildNavMesh();
        }
    }

    public void _LoadData(DoorData data)
    {
        _ApplyOpened(data.opened);
    }

    public void _ApplyOpened(bool opened)
    {
        this.opened = opened;

        if(opened)
        {
            if (doorCollider) doorCollider.enabled = false;
            for (int i = 0; i < doorMeshes.Length; i++)
            {
                Transform doorMesh = doorMeshes[i];

                doorMesh.localEulerAngles = new Vector3(doorMesh.eulerAngles.x, openedAngles[i], doorMesh.eulerAngles.z);
            }
        }
        else
        {
            if (doorCollider) doorCollider.enabled = true;
            for (int i = 0; i < doorMeshes.Length; i++)
            {
                Transform doorMesh = doorMeshes[i];

                doorMesh.localEulerAngles = new Vector3(doorMesh.eulerAngles.x, closedAngles[i], doorMesh.eulerAngles.z);
            }
        }
    }

    public DoorData _GetDoorData()
    {
        return new DoorData(opened, false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<PCController>() != null)
        {
            PCController pcController = other.GetComponent<PCController>();
            SetTransitionSystem.instance.ExecuteSetTransition(this, pcController);
        }
    }
}
