using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeatableObjBehavior : InteractableObjBehavior
{
    public List<SeatablePosition> seatablePositions;
    SeatablePosition currentSeatablePosition;

    public VIDE_Assign occupiedSeatsComment;

    bool playerSeated;
    bool leaveSeat;

    public virtual IEnumerator UseSeat()
    {
        SeatablePosition seatablePosition = null;
        foreach(SeatablePosition position in seatablePositions)
        {
            if(!position.occupied)
            {
                seatablePosition = position;
                break;
            }
        }

        if(seatablePosition != null)
        {
            currentSeatablePosition = seatablePosition;
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

            playerSeated = true;
            leaveSeat = false;

            while(!leaveSeat)
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
            DialogueUIController.PrepareDialogueUI(this, occupiedSeatsComment);
            yield return StartCoroutine(_BeginDialogue(occupiedSeatsComment));
        }
    }

    public void StartDisplaceToSeat()
    {
        Vector3 auxPosition = currentSeatablePosition.transform.position;
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

    private void Update()
    {
        if(playerSeated && Input.GetKeyDown(KeyCode.Space))
        {
            leaveSeat = true;
        }
    }
}

[System.Serializable]
public class SeatablePosition
{
    public Transform transform;
    public bool occupied;
}