using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenableEmitterObjBehavior : EmitterObjBehavior
{
    public string openAnimationTrigger = "open";
    public string closeAnimationTrigger = "close";

    public VIDE_Assign inspectComment;

    private Animator animator;
    public Animator Animator
    {
        get
        {
            if (animator == null) animator = GetComponent<Animator>();
            return animator;
        }
    }

    public virtual IEnumerator OpenMethod()
    {
        Animator.SetTrigger(openAnimationTrigger);

        yield return StartCoroutine(DropObjs(PlayPickAnimation()));

        Animator.SetTrigger(closeAnimationTrigger);
    }

    public virtual IEnumerator InspectInside()
    {
        Animator.SetTrigger(openAnimationTrigger);

        if(inspectComment != null)
        {
            DialogueUIController.PrepareDialogueUI(this, inspectComment);
            yield return StartCoroutine(_BeginDialogue(inspectComment));
        }

        Animator.SetTrigger(closeAnimationTrigger);
    }
}
