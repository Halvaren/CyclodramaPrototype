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

        opened = false;
        doorSign.gameObject.SetActive(false);
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

    public IEnumerator OpenDoor()
    {
        if (!opened && !openningClosing)
        {
            openningClosing = true; 
            for (int i = 0; i < doorMeshes.Length; i++)
            {
                if(i == doorMeshes.Length - 1)
                    yield return StartCoroutine(OpenCloseDoorCoroutine(doorMeshes[i], closedAngles[i], openedAngles[i], 0.5f, true));
                else
                    StartCoroutine(OpenCloseDoorCoroutine(doorMeshes[i], closedAngles[i], openedAngles[i], 0.5f));
            }
        }
            
    }

    public IEnumerator CloseDoor()
    {
        if (opened && !openningClosing)
        {
            openningClosing = true;
            for (int i = 0; i < doorMeshes.Length; i++)
            {
                if (i == doorMeshes.Length - 1)
                    yield return StartCoroutine(OpenCloseDoorCoroutine(doorMeshes[i], openedAngles[i], closedAngles[i], 0.5f, true));
                else
                    StartCoroutine(OpenCloseDoorCoroutine(doorMeshes[i], openedAngles[i], closedAngles[i], 0.5f));
            }
        }
    }

    IEnumerator OpenCloseDoorCoroutine(Transform doorMesh, float initialAngle, float finalAngle, float time, bool last = false)
    {
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

        if(last)
        {
            opened = !opened;
            openningClosing = false;

            if (obstacleCollider) obstacleCollider.enabled = !opened;

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
            if (obstacleCollider) obstacleCollider.enabled = false;
            for (int i = 0; i < doorMeshes.Length; i++)
            {
                Transform doorMesh = doorMeshes[i];

                doorMesh.localEulerAngles = new Vector3(doorMesh.eulerAngles.x, openedAngles[i], doorMesh.eulerAngles.z);
            }
        }
        else
        {
            if (obstacleCollider) obstacleCollider.enabled = true;
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
