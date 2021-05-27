using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GolfClubObjBehavior : PickableObjBehavior
{
    TeddyBearObjBehavior teddyBear;

    public override IEnumerator UseMethod(InteractableObjBehavior targetObj)
    {
        int index = GetObjRelationIndex(targetObj, useObjRelations);

        if (index == -1)
        {
            Debug.Log("Error");
        }
        else if (index == 0)
        {
            DialogueUIController.PrepareDialogueUI(this, defaultUseComment);
            yield return StartCoroutine(_BeginDialogue(defaultUseComment));
        }
        else if (index == 1)
        {
            teddyBear = (TeddyBearObjBehavior)targetObj;
            if (!teddyBear.fallen)
            {
                AddAnimationLock();
                PCController.secondAnimationCallback += BringTeddyBearDown;
                PCController.mainAnimationCallback += ReleaseAnimationLock;
                PCController.AnimationController.ReachWithGolfClub();

                while (animationLocks.Count > 0)
                {
                    yield return null;
                }

                PCController.mainAnimationCallback -= ReleaseAnimationLock;
                PCController.secondAnimationCallback -= BringTeddyBearDown;
                teddyBear.mainAnimationCallback -= ReleaseAnimationLock;

                teddyBear.SetFallen(true);
            }
            else
            {
                DialogueUIController.PrepareDialogueUI(this, defaultUseComment);
                yield return StartCoroutine(_BeginDialogue(defaultUseComment));
            }
            teddyBear = null;
        }

        yield return null;
    }

    void BringTeddyBearDown()
    {
        AddAnimationLock();
        teddyBear.mainAnimationCallback += ReleaseAnimationLock;
        teddyBear.Fall();
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
