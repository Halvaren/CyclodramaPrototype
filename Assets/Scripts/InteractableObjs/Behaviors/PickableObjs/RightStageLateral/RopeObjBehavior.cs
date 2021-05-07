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
    [Space(10)]
    public VIDE_Assign cannotPickComment;

    [Header("Other variables")]
    public BoxCollider secondTriggerCollider;

    private Animator animator;
    public Animator Animator
    {
        get
        {
            if (animator == null) animator = GetComponent<Animator>();
            return animator;
        }
    }

    protected override void InitializeObjBehavior()
    {
        base.InitializeObjBehavior();

        if(triggerCollider != null)
        {
            triggerCollider.enabled = !cut;
            secondTriggerCollider.enabled = cut;
        }
    }

    public override UseOfVerb GetUseOfVerb(ActionVerb verb)
    {
        UseOfVerb originalUseOfVerb = base.GetUseOfVerb(verb);

        if(originalUseOfVerb != null)
        {
            UseOfVerb useOfVerb = originalUseOfVerb.CopyUseOfVerb();

            if (useOfVerb.multiObj)
                useOfVerb.multiObj = cut;

            return useOfVerb;
        }
        return null;
    }

    public override IEnumerator _GetPicked()
    {
        if(!cut)
        {
            DialogueUIController.PrepareDialogueUI(this, cannotPickComment);
            yield return StartCoroutine(BeginDialogue(cannotPickComment));
        }
        else
        {
            yield return base._GetPicked();
        }
    }

    public override IEnumerator UseMethod(InteractableObjBehavior targetObj)
    {
        if(!cut)
        {
            DialogueUIController.PrepareDialogueUI(this, uncutDefaultUseComment);
            yield return StartCoroutine(BeginDialogue(uncutDefaultUseComment));
        }
        else
        {
            int index = GetObjRelationIndex(targetObj, useObjRelations);

            if(index == 0)
            {
                DialogueUIController.PrepareDialogueUI(this, cutDefaultUseComment);
                yield return StartCoroutine(BeginDialogue(cutDefaultUseComment));
            }
        }

        yield return null;
    }

    public override IEnumerator DrawMethod(InteractableObjBehavior targetObj)
    {
        if(!cut)
        {
            DialogueUIController.PrepareDialogueUI(this, uncutDefaultDrawComment);
            yield return StartCoroutine(BeginDialogue(uncutDefaultDrawComment));
        }
        else
        {
            int index = GetObjRelationIndex(targetObj, drawObjRelations);

            if (index == 0)
            {
                DialogueUIController.PrepareDialogueUI(this, cutDefaultDrawComment);
                yield return StartCoroutine(BeginDialogue(cutDefaultDrawComment));
            }
        }        

        yield return null;
    }

    public override IEnumerator GiveMethod(InteractableObjBehavior targetObj)
    {
        if(!cut)
        {
            DialogueUIController.PrepareDialogueUI(this, uncutDefaultGiveComment);
            yield return StartCoroutine(BeginDialogue(uncutDefaultGiveComment));
        }
        else
        {
            int index = GetObjRelationIndex(targetObj, giveObjRelations);

            if (index == 0)
            {
                DialogueUIController.PrepareDialogueUI(this, cutDefaultGiveComment);
                yield return StartCoroutine(BeginDialogue(cutDefaultGiveComment));
            }
        }

        yield return null;
    }

    public override IEnumerator HitMethod(InteractableObjBehavior targetObj)
    {
        if(!cut)
        {
            DialogueUIController.PrepareDialogueUI(this, uncutDefaultHitComment);
            yield return StartCoroutine(BeginDialogue(uncutDefaultHitComment));
        }
        else
        {
            int index = GetObjRelationIndex(targetObj, hitObjRelations);

            if (index == 0)
            {
                DialogueUIController.PrepareDialogueUI(this, cutDefaultHitComment);
                yield return StartCoroutine(BeginDialogue(cutDefaultHitComment));
            }
        }

        yield return null;
    }

    public override IEnumerator ThrowMethod(InteractableObjBehavior targetObj)
    {
        if(!cut)
        {
            DialogueUIController.PrepareDialogueUI(this, uncutDefaultThrowComment);
            yield return StartCoroutine(BeginDialogue(uncutDefaultThrowComment));
        }
        else
        {
            int index = GetObjRelationIndex(targetObj, throwObjRelations);

            if (index == 0)
            {
                DialogueUIController.PrepareDialogueUI(this, cutDefaultThrowComment);
                yield return StartCoroutine(BeginDialogue(cutDefaultThrowComment));
            }
        }

        yield return null;
    }

    public void Fall()
    {
        Animator.SetTrigger("Fall");
    }

    public void SetCut(bool value)
    {
        cut = value;
        triggerCollider.enabled = !cut;
        secondTriggerCollider.enabled = cut;
    }
}
