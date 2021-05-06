using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnifeObjBehavior : PickableObjBehavior
{
    //[Header("Object state")]

    [Header("Multi-object verbs fields")]
    public VIDE_Assign defaultUseComment;
    [Space(10)]
    public VIDE_Assign defaultDrawComment;
    [Space(10)]
    public VIDE_Assign defaultGiveComment;
    [Space(10)]
    public VIDE_Assign defaultHitComment;
    [Space(10)]
    public VIDE_Assign defaultThrowComment;

    public override IEnumerator UseMethod(InteractableObjBehavior targetObj)
    {
        int index = GetObjRelationIndex(targetObj, useObjRelations);

        if (index == -1)
        {
            Debug.Log("Error");
        }
        else if(index == 0)
        {
            DialogueUIController.StartDialogue(this, defaultUseComment);
        }
        else if(index == 1)
        {
            PCController.instance.AnimationController.UseKnife();
            RopeObjBehavior rope = (RopeObjBehavior)targetObj;
            rope.cut = true;
        }

        yield return null;
    }

    public override IEnumerator GiveMethod(InteractableObjBehavior targetObj)
    {
        int index = GetObjRelationIndex(targetObj, giveObjRelations);

        if (index == -1)
        {
            Debug.Log("Error");
        }
        else if (index == 0)
        {
            DialogueUIController.StartDialogue(this, defaultGiveComment);
        }

        yield return null;
    }

    public override IEnumerator HitMethod(InteractableObjBehavior targetObj)
    {
        int index = GetObjRelationIndex(targetObj, hitObjRelations);

        if (index == -1)
        {
            Debug.Log("Error");
        }
        else if (index == 0)
        {
            DialogueUIController.StartDialogue(this, defaultHitComment);
        }

        yield return null;
    }

    public override IEnumerator DrawMethod(InteractableObjBehavior targetObj)
    {
        int index = GetObjRelationIndex(targetObj, drawObjRelations);

        if (index == -1)
        {
            Debug.Log("Error");
        }
        else if (index == 0)
        {
            DialogueUIController.StartDialogue(this, defaultDrawComment);
        }

        yield return null;
    }

    public override IEnumerator ThrowMethod(InteractableObjBehavior targetObj)
    {
        int index = GetObjRelationIndex(targetObj, throwObjRelations);

        if (index == -1)
        {
            Debug.Log("Error");
        }
        else if (index == 0)
        {
            DialogueUIController.StartDialogue(this, defaultThrowComment);
        }

        yield return null;
    }
}
