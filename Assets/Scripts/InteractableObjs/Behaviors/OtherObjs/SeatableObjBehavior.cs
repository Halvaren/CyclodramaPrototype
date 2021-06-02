using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeatableObjBehavior : InteractableObjBehavior
{
    public SeatablePosition seatablePosition;

    public VIDE_Assign occupiedSeatsComment;

    private InputManager inputManager;
    public InputManager InputManager
    {
        get
        {
            if (inputManager == null) inputManager = InputManager.instance;
            return inputManager;
        }
    }

    public virtual IEnumerator UseSeat()
    {
        if(seatablePosition != null && !seatablePosition.occupied)
        {
            yield return StartCoroutine(PCController.MovementController.RotateToDirectionCoroutine(PCController.transform.position - seatablePosition.transform.position));

            AddAnimationLock();
            PCController.mainAnimationCallback += ReleaseAnimationLock;
            PCController.secondAnimationCallback += StartDisplaceToSeat;
            PCController.AnimationController.Seat();

            while (animationLocks.Count > 0)
            {
                yield return null;
            }

            PCController.mainAnimationCallback -= ReleaseAnimationLock;
            PCController.secondAnimationCallback -= StartDisplaceToSeat;

            while(!InputManager.pressedSpace)
            {
                yield return null;
            }

            AddAnimationLock();
            PCController.mainAnimationCallback += ReleaseAnimationLock;
            PCController.secondAnimationCallback += StartDisplaceToStandUp;
            PCController.AnimationController.StandUp();

            while (animationLocks.Count > 0)
            {
                yield return null;
            }

            PCController.mainAnimationCallback -= ReleaseAnimationLock;
            PCController.secondAnimationCallback -= StartDisplaceToStandUp;
        }
        else
        {
            yield return StartCoroutine(_StartConversation(occupiedSeatsComment));
        }
    }

    public void StartDisplaceToSeat()
    {
        Vector3 auxPosition = seatablePosition.transform.position;
        auxPosition.y = PCController.transform.position.y;
        StartCoroutine(DisplaceCharacter(PCController.transform.position, auxPosition, 1f));
    }

    public void StartDisplaceToStandUp()
    {
        Vector3 auxPosition = interactionPoint.transform.position;
        auxPosition.y = PCController.transform.position.y;
        StartCoroutine(DisplaceCharacter(PCController.transform.position, auxPosition, 1f));
    }

    IEnumerator DisplaceCharacter(Vector3 initialPosition, Vector3 finalPosition, float time)
    {
        float elapsedTime = 0.0f;

        while(elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;

            PCController.transform.position = Vector3.Lerp(initialPosition, finalPosition, elapsedTime / time);

            yield return null;
        }
        PCController.transform.position = finalPosition;
    }

    public override void LoadData(InteractableObjData data)
    {
        base.LoadData(data);

        if(data is SeatableObjData seatableObjData)
        {
            seatablePosition.occupied = seatableObjData.occupied;
        }
    }

    public override InteractableObjData GetObjData()
    {
        return new SeatableObjData(inScene, seatablePosition.occupied);
    }
}

[System.Serializable]
public class SeatablePosition
{
    public Transform transform;
    public bool occupied;
}