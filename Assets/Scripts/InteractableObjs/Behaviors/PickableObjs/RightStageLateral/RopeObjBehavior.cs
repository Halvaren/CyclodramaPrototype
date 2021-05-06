using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeObjBehavior : PickableObjBehavior
{
    [Header("Object state")]
    public bool cut = false;

    [Header("Multi-object verbs fields")]
    public VIDE_Assign cutDefaultUseComment;
    public VIDE_Assign uncutDefaultUseComment;
    [Space(10)]
    public VIDE_Assign cutDefaultDrawComment;
    public VIDE_Assign uncutDefaultDrawComment;
    [Space(10)]
    public VIDE_Assign cutDefaultGiveComment;
    public VIDE_Assign uncutDefaultGiveComment;
    [Space(10)]
    public VIDE_Assign cutDefaultHitComment;
    public VIDE_Assign uncutDefaultHitComment;
    [Space(10)]
    public VIDE_Assign cutDefaultThrowComment;
    public VIDE_Assign uncutDefaultThrowComment;

    public override UseOfVerb _GetUseOfVerb(ActionVerb verb)
    {
        UseOfVerb useOfVerb = base._GetUseOfVerb(verb).CopyUseOfVerb();
        useOfVerb.multiObj = cut;
        return useOfVerb;
    }

    public override IEnumerator UseMethod(InteractableObjBehavior targetObj)
    {
        if(!cut)
        {
            DialogueUIController.StartDialogue(this, uncutDefaultUseComment);
        }
        else
        {
            int index = GetObjRelationIndex(targetObj, useObjRelations);

            if(index == 0)
            {
                DialogueUIController.StartDialogue(this, cutDefaultUseComment);
            }
        }

        yield return null;
    }

    public override IEnumerator DrawMethod(InteractableObjBehavior targetObj)
    {
        if(!cut)
        {
            DialogueUIController.StartDialogue(this, uncutDefaultDrawComment);
        }
        else
        {
            int index = GetObjRelationIndex(targetObj, drawObjRelations);

            if (index == 0)
            {
                DialogueUIController.StartDialogue(this, cutDefaultDrawComment);
            }
        }        

        yield return null;
    }

    public override IEnumerator GiveMethod(InteractableObjBehavior targetObj)
    {
        if(!cut)
        {
            DialogueUIController.StartDialogue(this, uncutDefaultGiveComment);
        }
        else
        {
            int index = GetObjRelationIndex(targetObj, giveObjRelations);

            if (index == 0)
            {
                DialogueUIController.StartDialogue(this, cutDefaultGiveComment);
            }
        }

        yield return null;
    }

    public override IEnumerator HitMethod(InteractableObjBehavior targetObj)
    {
        if(!cut)
        {
            DialogueUIController.StartDialogue(this, uncutDefaultHitComment);
        }
        else
        {
            int index = GetObjRelationIndex(targetObj, hitObjRelations);

            if (index == 0)
            {
                DialogueUIController.StartDialogue(this, cutDefaultHitComment);
            }
        }

        yield return null;
    }

    public override IEnumerator ThrowMethod(InteractableObjBehavior targetObj)
    {
        if(!cut)
        {
            DialogueUIController.StartDialogue(this, uncutDefaultThrowComment);
        }
        else
        {
            int index = GetObjRelationIndex(targetObj, throwObjRelations);

            if (index == 0)
            {
                DialogueUIController.StartDialogue(this, cutDefaultThrowComment);
            }
        }

        yield return null;
    }
}
