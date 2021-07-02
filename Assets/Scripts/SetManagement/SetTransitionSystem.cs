using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Set transition movement directions
/// </summary>
public enum SetMovement
{
    Left, Right, Up, Down, Towards, Backwards
}

/// <summary>
/// Type of movement of the PC during a set transition
/// </summary>
public enum CharacterTransitionMovement
{
    LinearMovement, WaitAtPoint, FollowWaypoints
}

/// <summary>
/// Manages the transitions between sets
/// </summary>
public class SetTransitionSystem : MonoBehaviour
{
    #region Variables

    public Transform setOnStagePosition;

    bool setsMoving = false;
    bool pcMoving = false;

    public float rotationTime;
    public float offsetDisplacementTime;
    public float setDisplacementTime;
    public float setWaitingForPlayerTime;

    private GameManager gameManager;
    public GameManager GameManager
    {
        get
        {
            if (gameManager == null) gameManager = GameManager.instance;
            return gameManager;
        }
    }

    private PCController oliver;
    public PCController Oliver
    {
        get
        {
            if (oliver == null) oliver = PCController.instance;
            return oliver;
        }
    }

    private TheaterGearsBehavior gears;
    public TheaterGearsBehavior Gears
    {
        get
        {
            if (gears == null) gears = TheaterGearsBehavior.instance;
            return gears;
        }
    }

    public static SetTransitionSystem instance;

    #endregion

    private void Awake()
    {
        instance = this;
    }

    #region Methods

    #region Movement initializers

    /// <summary>
    /// Creates the initial set and moves it in the stage
    /// </summary>
    /// <param name="initialSetPrefab"></param>
    /// <param name="distanceToStagePosition"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    public SetBehavior InstantiateInitialSet(GameObject initialSetPrefab, float distanceToStagePosition, float time)
    {
        Vector3 setDisplacement = Vector3.zero;
        CalculateDisplacement(SetMovement.Down, distanceToStagePosition, ref setDisplacement);

        Transform initialSet = Instantiate(initialSetPrefab, setOnStagePosition.position - setDisplacement, Quaternion.identity).transform;

        StartCoroutine(MovementCoroutine(initialSet, initialSet.position + setDisplacement, time));

        return initialSet.GetComponent<SetBehavior>();
    }

    /// <summary>
    /// Starts a transition between two sets
    /// </summary>
    /// <param name="trigger"></param>
    public void ExecuteSetTransition(SetDoorBehavior trigger)
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

                nextTrigger.currentSet.GetComponent<SetBehavior>().OnInstantiate();

                switch (trigger.characterTransitionMovement)
                {
                    case CharacterTransitionMovement.LinearMovement:
                        if (trigger.characterWaitsUntilSetMovementIsDone)
                            StartCoroutine(SetTransitionLinearMovementWithWaitingCoroutine(trigger, nextTrigger, 
                                currentSetDisplacement, nextSetDisplacement, currentSet, nextSet, trigger.setTransitionMovement));
                        else
                            StartCoroutine(SetTransitionLinearMovementWithoutWaitingCoroutine(trigger, nextTrigger, 
                                currentSetDisplacement, nextSetDisplacement, currentSet, nextSet, trigger.setTransitionMovement));
                        break;
                    case CharacterTransitionMovement.WaitAtPoint:
                        StartCoroutine(SetTransitionWaitAtPointCoroutine(trigger, nextTrigger, currentSetDisplacement, currentSet, nextSet));
                        break;
                    case CharacterTransitionMovement.FollowWaypoints:
                        StartCoroutine(SetTransitionFollowWaypointsCoroutine(trigger, nextTrigger, currentSetDisplacement, currentSet, nextSet));
                        break;
                }
            }
        }
    }

    /// <summary>
    /// Moves final set out of the stage
    /// </summary>
    /// <param name="finalSet"></param>
    /// <param name="distanceToEndPosition"></param>
    /// <param name="time"></param>
    public void DisappearFinalSet(Transform finalSet, float distanceToEndPosition, float time)
    {
        Vector3 setDisplacement = Vector3.zero;
        CalculateDisplacement(SetMovement.Up, distanceToEndPosition, ref setDisplacement);

        StartCoroutine(MovementCoroutine(finalSet, finalSet.position + setDisplacement, time));
    }

    #endregion

    #region Set transition coroutines

    /// <summary>
    /// Executes transition between sets with PC moving linearly and without waiting
    /// </summary>
    /// <param name="currentTrigger"></param>
    /// <param name="nextTrigger"></param>
    /// <param name="currentSetDisplacement"></param>
    /// <param name="nextSetDisplacement"></param>
    /// <param name="currentSet"></param>
    /// <param name="nextSet"></param>
    /// <param name="setMovement"></param>
    /// <returns></returns>
    IEnumerator SetTransitionLinearMovementWithoutWaitingCoroutine(SetDoorBehavior currentTrigger, SetDoorBehavior nextTrigger,
        Vector3 currentSetDisplacement, Vector3 nextSetDisplacement, Transform currentSet, Transform nextSet, SetMovement setMovement)
    {
        Gears.TurnOnOffGears(true);
        Oliver.PrepareForMovementBetweenSets(true);

        Oliver.transform.parent = currentSet;

        if (currentTrigger.offset != Vector3.zero) yield return StartCoroutine(MovementCoroutine(currentSet, setOnStagePosition.position + currentTrigger.offset, offsetDisplacementTime));
        if (currentTrigger.rotation != 0f) 
        {
            Gears.PlayGearsRotationSound();
            yield return StartCoroutine(TurnCoroutine(currentSet, -currentTrigger.rotation, rotationTime)); 
        }

        Oliver.transform.parent = null;

        pcMoving = true;
        Oliver.LinearMovementBetweenSets(nextSet, nextTrigger.characterFinalPosition, currentTrigger.characterXMovement, currentTrigger.characterYMovement, currentTrigger.characterZMovement);
        yield return new WaitForSeconds(setWaitingForPlayerTime);

        Gears.PlayGearsMovementSound(setMovement);
        StartCoroutine(MovementCoroutine(currentSet, currentSet.position + currentSetDisplacement, setDisplacementTime));
        yield return StartCoroutine(MovementCoroutine(nextSet, nextSet.position + nextSetDisplacement, setDisplacementTime));

        currentSet.GetComponent<NavMeshSurface>().RemoveData();

        DestroyImmediate(currentSet.gameObject);

        while (pcMoving)
            yield return null;

        if (nextTrigger.rotation != 0f) 
        {
            Gears.PlayGearsRotationSound();
            yield return StartCoroutine(TurnCoroutine(nextSet, nextTrigger.rotation, rotationTime)); 
        }
        if (nextTrigger.offset != Vector3.zero) yield return StartCoroutine(MovementCoroutine(nextSet, setOnStagePosition.position, offsetDisplacementTime));

        Gears.TurnOnOffGears(false);

        SetTransitionDone(nextSet);
    }

    /// <summary>
    /// Executes transition between sets with PC moving linearly and with waiting
    /// </summary>
    /// <param name="currentTrigger"></param>
    /// <param name="nextTrigger"></param>
    /// <param name="currentSetDisplacement"></param>
    /// <param name="nextSetDisplacement"></param>
    /// <param name="currentSet"></param>
    /// <param name="nextSet"></param>
    /// <param name="setMovement"></param>
    /// <returns></returns>
    IEnumerator SetTransitionLinearMovementWithWaitingCoroutine(SetDoorBehavior currentTrigger, SetDoorBehavior nextTrigger,
        Vector3 currentSetDisplacement, Vector3 nextSetDisplacement, Transform currentSet, Transform nextSet, SetMovement setMovement)
    {
        Gears.TurnOnOffGears(true);
        Oliver.PrepareForMovementBetweenSets(true);

        Oliver.transform.parent = currentSet;

        if (currentTrigger.offset != Vector3.zero)
        {
            Gears.PlayGearsRotationSound();
            yield return StartCoroutine(MovementCoroutine(currentSet, setOnStagePosition.position + currentTrigger.offset, offsetDisplacementTime));
        }
        if (currentTrigger.rotation != 0f) yield return StartCoroutine(TurnCoroutine(currentSet, -currentTrigger.rotation, rotationTime));

        Gears.PlayGearsMovementSound(setMovement);
        StartCoroutine(MovementCoroutine(currentSet, currentSet.position + currentSetDisplacement, setDisplacementTime));
        yield return StartCoroutine(MovementCoroutine(nextSet, nextSet.position + nextSetDisplacement, setDisplacementTime));

        currentSet.GetComponent<NavMeshSurface>().RemoveData();

        Oliver.transform.parent = null;

        DestroyImmediate(currentSet.gameObject);

        pcMoving = true;
        Oliver.LinearMovementBetweenSets(nextSet, nextTrigger.characterFinalPosition, currentTrigger.characterXMovement, currentTrigger.characterYMovement, currentTrigger.characterZMovement);

        while (pcMoving)
            yield return null;

        if (nextTrigger.rotation != 0f) 
        {
            Gears.PlayGearsRotationSound();
            yield return StartCoroutine(TurnCoroutine(nextSet, nextTrigger.rotation, rotationTime)); 
        }
        if (nextTrigger.offset != Vector3.zero) yield return StartCoroutine(MovementCoroutine(nextSet, setOnStagePosition.position, offsetDisplacementTime));

        Gears.TurnOnOffGears(false);

        SetTransitionDone(nextSet);
    }

    /// <summary>
    /// Executes transition between sets with PC waiting at a point
    /// </summary>
    /// <param name="trigger"></param>
    /// <param name="nextTrigger"></param>
    /// <param name="currentSetDisplacement"></param>
    /// <param name="currentSet"></param>
    /// <param name="nextSet"></param>
    /// <returns></returns>
    IEnumerator SetTransitionWaitAtPointCoroutine(SetDoorBehavior trigger, SetDoorBehavior nextTrigger,
        Vector3 currentSetDisplacement, Transform currentSet, Transform nextSet)
    {
        Oliver.PrepareForMovementBetweenSets(false);

        Transform waitPosition = trigger.characterWaitPosition;
        waitPosition.parent = null;

        pcMoving = true;
        Oliver.MoveToPoint(waitPosition);
        yield return new WaitForSeconds(setWaitingForPlayerTime);

        StartCoroutine(MovementCoroutine(currentSet, setOnStagePosition.position + currentSetDisplacement, setDisplacementTime));
        yield return StartCoroutine(MovementCoroutine(nextSet, setOnStagePosition.position, setDisplacementTime));

        currentSet.GetComponent<NavMeshSurface>().RemoveData();

        DestroyImmediate(currentSet.gameObject);

        while (pcMoving)
            yield return null;

        Destroy(waitPosition.gameObject);

        pcMoving = true;
        Oliver.MoveToPoint(nextTrigger.characterFinalPosition);

        while (pcMoving)
            yield return null;

        if (trigger.rotation != 0f) yield return StartCoroutine(TurnCoroutine(nextSet, trigger.rotation, rotationTime));

        SetTransitionDone(nextSet);
    }

    /// <summary>
    /// Executes transition between sets with PC following a list of waypoints
    /// </summary>
    /// <param name="trigger"></param>
    /// <param name="nextTrigger"></param>
    /// <param name="currentSetDisplacement"></param>
    /// <param name="currentSet"></param>
    /// <param name="nextSet"></param>
    /// <returns></returns>
    IEnumerator SetTransitionFollowWaypointsCoroutine(SetDoorBehavior trigger, SetDoorBehavior nextTrigger,
        Vector3 currentSetDisplacement, Transform currentSet, Transform nextSet)
    {
        Oliver.PrepareForMovementBetweenSets(false);

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
        Oliver.FollowWaypoints(waypoints, nextSet);
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

        SetTransitionDone(nextSet);
    }

    #endregion

    #region Movement and rotation coroutines

    /// <summary>
    /// Moves a set to a destination during a time
    /// </summary>
    /// <param name="set"></param>
    /// <param name="destination"></param>
    /// <param name="time"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Turns a set an amount of degrees (rotation) during a time
    /// </summary>
    /// <param name="set"></param>
    /// <param name="rotation"></param>
    /// <param name="time"></param>
    /// <returns></returns>
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

    #endregion

    #region Other methods

    /// <summary>
    /// Calculates the displacement of a set has to move depending on the SetMovement
    /// </summary>
    /// <param name="movement"></param>
    /// <param name="distance"></param>
    /// <param name="setDisplacement"></param>
    void CalculateDisplacement(SetMovement movement, float distance, ref Vector3 setDisplacement)
    {
        switch(movement)
        {
            case SetMovement.Right:
                setDisplacement = distance * -Vector3.left;
                break;
            case SetMovement.Left:
                setDisplacement = distance * -Vector3.right;
                break;
            case SetMovement.Up:
                setDisplacement = distance * Vector3.down;
                break;
            case SetMovement.Down:
                setDisplacement = distance * Vector3.up;
                break;
            case SetMovement.Backwards:
                setDisplacement = distance * -Vector3.back;
                break;
            case SetMovement.Towards:
                setDisplacement = distance * Vector3.up;
                break;
        }
    }

    /// <summary>
    /// Calculates the displacement of a pair of sets depending on the SetMovement
    /// </summary>
    /// <param name="movement"></param>
    /// <param name="distanceBetweenSets"></param>
    /// <param name="currentSetDisplacement"></param>
    /// <param name="nextSetDisplacement"></param>
    void CalculateDisplacements(SetMovement movement, float distanceBetweenSets, ref Vector3 currentSetDisplacement, ref Vector3 nextSetDisplacement)
    {
        switch (movement)
        {
            case SetMovement.Right:
                currentSetDisplacement = distanceBetweenSets * -Vector3.left;
                nextSetDisplacement = distanceBetweenSets * -Vector3.left;
                break;
            case SetMovement.Left:
                currentSetDisplacement = distanceBetweenSets * -Vector3.right;
                nextSetDisplacement = distanceBetweenSets * -Vector3.right;
                break;
            case SetMovement.Up:
                currentSetDisplacement = distanceBetweenSets * Vector3.down;
                nextSetDisplacement = distanceBetweenSets * Vector3.down;
                break;
            case SetMovement.Down:
                currentSetDisplacement = distanceBetweenSets * Vector3.up;
                nextSetDisplacement = distanceBetweenSets * Vector3.up;
                break;
            case SetMovement.Backwards:
                currentSetDisplacement = distanceBetweenSets * Vector3.down;
                nextSetDisplacement = distanceBetweenSets * -Vector3.back;
                break;
            case SetMovement.Towards:
                currentSetDisplacement = distanceBetweenSets * -Vector3.forward;
                nextSetDisplacement = distanceBetweenSets * Vector3.up;
                break;
        }
    }

    /// <summary>
    /// Sets PC movement is done
    /// </summary>
    public void SetCharacterMovementDone()
    {
        pcMoving = false;
    }

    /// <summary>
    /// It is executed when the transition is finished for the sets
    /// </summary>
    /// <param name="nextSet"></param>
    void SetTransitionDone(Transform nextSet)
    {
        setsMoving = false;

        nextSet.GetComponent<SetBehavior>().OnAfterSetChanging();
    }

    /// <summary>
    /// Checks if a SetDoor is connected to another SetDoor from other set
    /// </summary>
    /// <param name="trigger"></param>
    /// <returns></returns>
    bool CheckIfConnected(SetDoorBehavior trigger)
    {
        if (GetNextTrigger(trigger, trigger.nextSet) != null) return true;
        return false;
    }

    /// <summary>
    /// Returns the SetDoor connected to the SetDoor passed as a parameter
    /// </summary>
    /// <param name="currentTrigger"></param>
    /// <param name="nextSet"></param>
    /// <returns></returns>
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

    #endregion

    #endregion
}
