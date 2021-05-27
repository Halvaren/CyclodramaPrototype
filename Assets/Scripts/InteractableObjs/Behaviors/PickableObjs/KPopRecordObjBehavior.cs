using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KPopRecordObjBehavior : PickableObjBehavior
{
    public bool notanPresent = true;

    public override IEnumerator GiveMethod(InteractableObjBehavior targetObj)
    {
        int index = GetObjRelationIndex(targetObj, giveObjRelations);

        //Belinda
        if(index == 1)
        {

        }

        yield return base.GiveMethod(targetObj);
    }

    public override IEnumerator _GetPicked()
    {
        if(notanPresent)
        {
            DialogueUIController.PrepareDialogueUI(this, cannotPickComment);
            yield return StartCoroutine(_BeginDialogue(cannotPickComment));
        }
        else
            yield return base._GetPicked();
    }
}
