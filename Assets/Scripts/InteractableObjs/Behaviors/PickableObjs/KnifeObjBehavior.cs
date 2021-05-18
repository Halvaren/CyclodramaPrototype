using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnifeObjBehavior : PickableObjBehavior
{
    //[Header("Object state")]

    public override IEnumerator UseMethod(InteractableObjBehavior targetObj)
    {
        int index = GetObjRelationIndex(targetObj, useObjRelations);

        if (index == -1)
        {
            Debug.Log("Error");
        }
        else if(index == 0)
        {
            DialogueUIController.PrepareDialogueUI(this, defaultUseComment);
            yield return StartCoroutine(_BeginDialogue(defaultUseComment));
        }
        else if(index == 1)
        {
            AddAnimationLock();
            PCController.mainAnimationCallback += ReleaseAnimationLock;
            PCController.AnimationController.UseKnife();

            while(animationLocks.Count > 0)
            {
                yield return null;
            }

            PCController.mainAnimationCallback -= ReleaseAnimationLock;

            RopeObjBehavior rope = (RopeObjBehavior)targetObj;
            AddAnimationLock();
            rope.animationCallback += ReleaseAnimationLock;
            rope.Fall();

            while(animationLocks.Count > 0)
            {
                yield return null;
            }

            rope.animationCallback -= ReleaseAnimationLock;
            rope.SetCut(true);
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
            DialogueUIController.PrepareDialogueUI(this, defaultGiveComment);
            yield return StartCoroutine(_BeginDialogue(defaultGiveComment));
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
            DialogueUIController.PrepareDialogueUI(this, defaultHitComment);
            yield return StartCoroutine(_BeginDialogue(defaultHitComment));
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
            DialogueUIController.PrepareDialogueUI(this, defaultDrawComment);
            yield return StartCoroutine(_BeginDialogue(defaultDrawComment));
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
            DialogueUIController.PrepareDialogueUI(this, defaultThrowComment);
            yield return StartCoroutine(_BeginDialogue(defaultThrowComment));
        }

        yield return null;
    }
}
