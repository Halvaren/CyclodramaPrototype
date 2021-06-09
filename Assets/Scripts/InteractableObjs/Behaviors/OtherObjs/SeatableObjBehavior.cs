using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SeatType
{
    Chair, Couch
}

public class SeatableObjBehavior : InteractableObjBehavior
{
    public SeatablePosition seatablePosition;
    public SeatType seatType;

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

    private ActionVerbsUIController actionVerbsUIController;
    public ActionVerbsUIController ActionVerbsUIController
    {
        get
        {
            if (actionVerbsUIController == null) actionVerbsUIController = GeneralUIController.instance.actionVerbsUIController;
            return actionVerbsUIController;
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
            PCController.AnimationController.Seat(seatType);

            while (animationLocks.Count > 0)
            {
                yield return null;
            }

            PCController.mainAnimationCallback -= ReleaseAnimationLock;
            PCController.secondAnimationCallback -= StartDisplaceToSeat;

            ActionVerbsUIController.ShowUnshowEscapeIcon(true);

            PCController.EnablePauseInput(false);

            while (!InputManager.pressedEscape)
            {
                yield return null;
            }

            PCController.EnablePauseInput(true);

            ActionVerbsUIController.ShowUnshowEscapeIcon(false);

            AddAnimationLock();
            PCController.mainAnimationCallback += ReleaseAnimationLock;
            PCController.secondAnimationCallback += StartDisplaceToStandUp;
            PCController.AnimationController.StandUp(seatType);

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