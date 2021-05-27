using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VillainDrawingObjBehavior : PickableObjBehavior
{
    public override IEnumerator GiveMethod(InteractableObjBehavior targetObj)
    {
        int index = GetObjRelationIndex(targetObj, giveObjRelations);

        //Belinda
        if(index == 1)
        {

        }
        else
        {
            yield return base.GiveMethod(targetObj);
        }
    }
}
