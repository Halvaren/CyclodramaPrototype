using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum SetTransitionMovement
{
    Left, Right, Up, Down, Towards, Backwards
}

public enum CharacterTransitionMovement
{
    LinearMovement, WaitAtPoint, FollowWaypoints
}

public class SetTransitionSystem : MonoBehaviour
{
    public Transform setOnStagePosition;

    public static SetTransitionSystem instance;

    bool setsMoving = false;
    bool pcMoving = false;

    public Transform initialSet;

    public float rotationTime;
    public float offsetDisplacementTime;
    public float setDisplacementTime;
    public float setWaitingForPlayerTime;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        initialSet.GetComponent<NavMeshSurface>().BuildNavMesh();
    }

    public void ExecuteSetTransition(SetDoorBehavior trigger, PCController playableCharacter)
    {
        if(!setsMoving && !pcMoving)
        {
            if(CheckIfConnected(trigger))
            {
                trigger.currentSet.GetComponent<SetBehavior>().OnBeforeSetChanging();

                setsMoving = true;

                Vector3 currentSetDisplacement = Vector3.zero;
                Vector3 nextSetDisplacement = Vector3.zero;
                CalculateDisplacements(trigger.setTransitionMovement, trigger.distanceBetweenSets, ref currentSetDisplacement, ref nextSetDisplacement);

                Transform currentSet = trigger.currentSet.transform;
                Transform nextSet = Instantiate(trigger.nextSet, setOnStagePosition.position - nextSetDisplacement, Quaternion.identity).transform;

                SetDoorBehavior nextTrigger = GetNextTrigger(trigger, nextSet.gameObject);
                nextSet.position += nextTrigger.offset;
                nextSet.eulerAngles = new Vector3(0f, -nextTrigger.rotation, 0f);

                nextTrigger.currentSet.GetComponent<SetBehavior>().TurnOnOffLights(false);

                switch (trigger.characterTransitionMovement)
                {
                    case CharacterTransitionMovement.LinearMovement:
                        if (trigger.characterWaitsUntilSetMovementIsDone)
                            StartCoroutine(SetTransitionLinearMovementWithWaitingCoroutine(trigger, nextTrigger, playableCharacter, 
                                currentSetDisplacement, nextSetDisplacement, currentSet, nextSet));
                        else
                            StartCoroutine(SetTransitionLinearMovementWithoutWaitingCoroutine(trigger, nextTrigger, playableCharacter, 
                                currentSetDisplacement, nextSetDisplacement, currentSet, nextSet));
                        break;
                    case CharacterTransitionMovement.WaitAtPoint:
                        StartCoroutine(SetTransitionWaitAtPointCoroutine(trigger, nextTrigger, playableCharacter, currentSetDisplacement, currentSet, nextSet));
                        break;
                    case CharacterTransitionMovement.FollowWaypoints:
                        StartCoroutine(SetTransitionFollowWaypointsCoroutine(trigger, nextTrigger, playableCharacter, currentSetDisplacement, currentSet, nextSet));
                        break;
                }
            }
        }
    }

    public void SetCharacterMovementDone()
    {
        pcMoving = false;
    }

    void SetTransitionDone(Transform nextSet, PCController playableCharacter)
    {
        setsMoving = false;

        nextSet.GetComponent<SetBehavior>().OnAfterSetChanging();

        playableCharacter.SetTransitionDone();
    }

    bool CheckIfConnected(SetDoorBehavior trigger)
    {
        if (GetNextTrigger(trigger, trigger.nextSet) != null) return true;
        return false;
    }

    SetDoorBehavior GetNextTrigger(SetDoorBehavior currentTrigger, GameObject nextSet)
    {
        SetDoorBehavior[] nextSetTriggers = nextSet.GetComponentsInChildren<SetDoorBehavior>();

        foreach (SetDoorBehavior nextSetTrigger in nextSetTriggers)
        {
            if (nextSetTrigger.connectionIndex != -1 && nextSetTrigger.connectionIndex == currentTrigger.connectionIndex)
            {
                return nextSetTrigger;
            }
        }

        return null;
    }

    IEnumerator SetTransitionLinearMovementWithoutWaitingCoroutine(SetDoorBehavior currentTrigger, SetDoorBehavior nextTrigger, PCController playableCharacter,
        Vector3 currentSetDisplacement, Vector3 nextSetDisplacement, Transform currentSet, Transform nextSet)
    {
        playableCharacter.PrepareForMovementBetweenSets(true);

        playableCharacter.transform.parent = currentSet;

        if (currentTrigger.offset != Vector3.zero) yield return StartCoroutine(MovementCoroutine(currentSet, setOnStagePosition.position + currentTrigger.offset, offsetDisplacementTime));
        if (currentTrigger.rotation != 0f) yield return StartCoroutine(TurnCoroutine(currentSet, -currentTrigger.rotation, rotationTime));

        playableCharacter.transform.parent = null;

        pcMoving = true;
        playableCharacter.LinearMovementBetweenSets(nextSet, nextTrigger.characterFinalPosition, currentTrigger.characterXMovement, currentTrigger.characterYMovement, currentTrigger.characterZMovement);
        yield return new WaitForSeconds(setWaitingForPlayerTime);

        StartCoroutine(MovementCoroutine(currentSet, currentSet.position + currentSetDisplacement, setDisplacementTime));
        yield return StartCoroutine(MovementCoroutine(nextSet, nextSet.position + nextSetDisplacement, setDisplacementTime));

        currentSet.GetComponent<NavMeshSurface>().RemoveData();

        DestroyImmediate(currentSet.gameObject);

        while (pcMoving)
            yield return null;

        if (nextTrigger.rotation != 0f) yield return StartCoroutine(TurnCoroutine(nextSet, nextTrigger.rotation, rotationTime));
        if (nextTrigger.offset != Vector3.zero) yield return StartCoroutine(MovementCoroutine(nextSet, setOnStagePosition.position, offsetDisplacementTime));

        SetTransitionDone(nextSet, playableCharacter);
    }

    IEnumerator SetTransitionLinearMovementWithWaitingCoroutine(SetDoorBehavior currentTrigger, SetDoorBehavior nextTrigger, PCController playableCharacter,
        Vector3 currentSetDisplacement, Vector3 nextSetDisplacement, Transform currentSet, Transform nextSet)
    {
        playableCharacter.PrepareForMovementBetweenSets(true);

        playableCharacter.transform.parent = currentSet;

        if (currentTrigger.offset != Vector3.zero) yield return StartCoroutine(MovementCoroutine(currentSet, setOnStagePosition.position + currentTrigger.offset, offsetDisplacementTime));
        if (currentTrigger.rotation != 0f) yield return StartCoroutine(TurnCoroutine(currentSet, -currentTrigger.rotation, rotationTime));

        StartCoroutine(MovementCoroutine(currentSet, currentSet.position + currentSetDisplacement, setDisplacementTime));
        yield return StartCoroutine(MovementCoroutine(nextSet, nextSet.position + nextSetDisplacement, setDisplacementTime));

        currentSet.GetComponent<NavMeshSurface>().RemoveData();

        playableCharacter.transform.parent = null;

        DestroyImmediate(currentSet.gameObject);

        pcMoving = true;
        playableCharacter.LinearMovementBetweenSets(nextSet, nextTrigger.characterFinalPosition, currentTrigger.characterXMovement, currentTrigger.characterYMovement, currentTrigger.characterZMovement);

        while (pcMoving)
            yield return null;

        if (nextTrigger.rotation != 0f) yield return StartCoroutine(TurnCoroutine(nextSet, nextTrigger.rotation, rotationTime));
        if (nextTrigger.offset != Vector3.zero) yield return StartCoroutine(MovementCoroutine(nextSet, setOnStagePosition.position, offsetDisplacementTime));

        SetTransitionDone(nextSet, playableCharacter);
    }

    IEnumerator SetTransitionWaitAtPointCoroutine(SetDoorBehavior trigger, SetDoorBehavior nextTrigger, PCController playableCharacter,
        Vector3 currentSetDisplacement, Transform currentSet, Transform nextSet)
    {
        playableCharacter.PrepareForMovementBetweenSets(false);

        Transform waitPosition = trigger.characterWaitPosition;
        waitPosition.parent = null;

        pcMoving = true;
        playableCharacter.MoveToPoint(waitPosition);
        yield return new WaitForSeconds(setWaitingForPlayerTime);

        StartCoroutine(MovementCoroutine(currentSet, setOnStagePosition.position + currentSetDisplacement, setDisplacementTime));
        yield return StartCoroutine(MovementCoroutine(nextSet, setOnStagePosition.position, setDisplacementTime));

        currentSet.GetComponent<NavMeshSurface>().RemoveData();

        DestroyImmediate(currentSet.gameObject);

        while (pcMoving)
            yield return null;

        Destroy(waitPosition.gameObject);

        pcMoving = true;
        playableCharacter.MoveToPoint(nextTrigger.characterFinalPosition);

        while (pcMoving)
            yield return null;

        if (trigger.rotation != 0f) yield return StartCoroutine(TurnCoroutine(nextSet, trigger.rotation, rotationTime));

        SetTransitionDone(nextSet, playableCharacter);
    }

    IEnumerator SetTransitionFollowWaypointsCoroutine(SetDoorBehavior trigger, SetDoorBehavior nextTrigger, PCController playableCharacter,
        Vector3 currentSetDisplacement, Transform currentSet, Transform nextSet)
    {
        playableCharacter.PrepareForMovementBetweenSets(false);

        List<Transform> waypoints = new List<Transform>();
        if(trigger.waypointsInNextTrigger)
        {
            for (int i = nextTrigger.characterWaypoints.Length - 1; i >= 0; i--)
                waypoints.Add(nextTrigger.characterWaypoints[i]);
        }
        else
        {
            foreach (Transform waypoint in trigger.characterWaypoints)
                waypoints.Add(waypoint);
        }
        waypoints.Add(nextTrigger.characterFinalPosition);

        pcMoving = true;
        playableCharacter.FollowWaypoints(waypoints, nextSet);
        yield return new WaitForSeconds(setWaitingForPlayerTime);

        StartCoroutine(MovementCoroutine(currentSet, setOnStagePosition.position + currentSetDisplacement, setDisplacementTime));
        yield return StartCoroutine(MovementCoroutine(nextSet, setOnStagePosition.position, setDisplacementTime));

        while (pcMoving)
        {
            yield return null;
        }

        currentSet.GetComponent<NavMeshSurface>().RemoveData();

        DestroyImmediate(currentSet.gameObject);

        if (trigger.rotation != 0f) yield return StartCoroutine(TurnCoroutine(nextSet, trigger.rotation, rotationTime));

        SetTransitionDone(nextSet, playableCharacter);
    }

    void CalculateDisplacements(SetTransitionMovement setTransitionMovement, float distanceBetweenSets, ref Vector3 currentSetDisplacement, ref Vector3 nextSetDisplacement)
    {
        switch (setTransitionMovement)
        {
            case SetTransitionMovement.Right:
                currentSetDisplacement = distanceBetweenSets * -Vector3.left;
                nextSetDisplacement = distanceBetweenSets * -Vector3.left;
                break;
            case SetTransitionMovement.Left:
                currentSetDisplacement = distanceBetweenSets * -Vector3.right;
                nextSetDisplacement = distanceBetweenSets * -Vector3.right;
                break;
            case SetTransitionMovement.Up:
                currentSetDisplacement = distanceBetweenSets * Vector3.down;
                nextSetDisplacement = distanceBetweenSets * Vector3.down;
                break;
            case SetTransitionMovement.Down:
                currentSetDisplacement = distanceBetweenSets * Vector3.up;
                nextSetDisplacement = distanceBetweenSets * Vector3.up;
                break;
            case SetTransitionMovement.Backwards:
                currentSetDisplacement = distanceBetweenSets * Vector3.down;
                nextSetDisplacement = distanceBetweenSets * -Vector3.back;
                break;
            case SetTransitionMovement.Towards:
                currentSetDisplacement = distanceBetweenSets * -Vector3.forward;
                nextSetDisplacement = distanceBetweenSets * Vector3.up;
                break;
        }
    }

    IEnumerator MovementCoroutine(Transform set, Vector3 destination, float time)
    {
        Vector3 initialPosition = set.position;

        float elapsedTime = 0.0f;

        while(elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;

            set.position = Vector3.Lerp(initialPosition, destination, elapsedTime / time);

            yield return null;
        }
        set.position = destination;
    }

    IEnumerator TurnCoroutine(Transform set, float rotation, float time)
    {
        Vector3 initialRotation = set.eulerAngles;
        Vector3 finalRotation = initialRotation + new Vector3(0f, rotation, 0f);

        float elapsedTime = 0.0f;

        while(elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;

            set.eulerAngles = Vector3.Lerp(initialRotation, finalRotation, elapsedTime / time);

            yield return null;
        }
        set.eulerAngles = finalRotation;
    }
}
