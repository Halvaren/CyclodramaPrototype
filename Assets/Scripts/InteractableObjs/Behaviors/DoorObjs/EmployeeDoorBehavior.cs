using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmployeeDoorBehavior : DoorBehavior
{
    public VIDE_Assign questsNotCompletedYet;
    public VIDE_Assign cantUnlockComment;
    public EmployeeDoorTrigger doorTrigger;

    public bool Act2Scene1QuestsCompleted
    {
        get
        {
            return PCController.pcData.givenBelindaInspiration && PCController.pcData.givenBelindaFabrics && PCController.pcData.gotNotanMeasurements;
        }
    }

    public override IEnumerator OpenDoor()
    {
        if(Act2Scene1QuestsCompleted)
        {
            yield return base.OpenDoor();
        }
        else
        {
            yield return StartCoroutine(_StartConversation(questsNotCompletedYet));
        }
    }

    public override IEnumerator ForceLock()
    {
        PlayLockedSound();
        yield return StartCoroutine(_StartConversation(cantUnlockComment));
    }

    public IEnumerator _OpenDoorBeginningNewScene()
    {
        yield return base.OpenDoor();
    }
}
