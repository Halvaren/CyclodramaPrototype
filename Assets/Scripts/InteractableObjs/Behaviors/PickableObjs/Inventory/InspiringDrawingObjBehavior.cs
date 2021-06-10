using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InspiringDrawingObjBehavior : PickableObjBehavior
{
    public override IEnumerator GiveMethod(InteractableObjBehavior targetObj)
    {
        int index = GetObjRelationIndex(targetObj, giveObjRelations);

        //Belinda
        if (index == 1)
        {
            BelindaBehavior belinda = (BelindaBehavior)targetObj;
            yield return belinda.StartCoroutine(belinda._GiveObj(obj));
        }
        else
        {
            yield return base.GiveMethod(targetObj);
        }
    }
}
