using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DoorBehavior : InteractableObjBehavior
{
    [HideInInspector]
    public bool opened = false;
    [HideInInspector]
    public bool locked = false;
    [HideInInspector]
    bool openningClosing = false;

    [HideInInspector]
    public float[] closedAngles;
    [HideInInspector]
    public float[] openedAngles;

    [HideInInspector]
    public Transform[] doorMeshes;
    [HideInInspector]
    public Transform doorSign;
    [HideInInspector]
    public Collider doorCollider;

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
    public GameObject currentSet;
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

    #region Data methods

    public void _LoadData(DoorData data)
    {
        _ApplyData(data.inScene, data.opened);
    }

    public void _ApplyData(bool inScene, bool opened)
    {
        _ApplyData(inScene);
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

    public override InteractableObjData _GetObjData()
    {
        return new DoorData(inScene, opened, locked);
    }

    #endregion

    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<PCController>() != null)
        {
            PCController pcController = other.GetComponent<PCController>();
            SetTransitionSystem.instance.ExecuteSetTransition(this, pcController);
        }
    }
}
