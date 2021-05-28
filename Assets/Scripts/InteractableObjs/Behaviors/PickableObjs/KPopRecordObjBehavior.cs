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
            BelindaBehavior belinda = (BelindaBehavior)targetObj;
            yield return belinda.StartCoroutine(belinda._GiveObj(obj));
        }

        yield return base.GiveMethod(targetObj);
    }

    public override IEnumerator _GetPicked()
    {
        if(notanPresent)
        {
            yield return StartCoroutine(_StartConversation(cannotPickComment));
        }
        else
            yield return base._GetPicked();
    }
}
