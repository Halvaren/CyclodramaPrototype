using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashCanEmitterObjBehavior : OpenableEmitterObjBehavior
{
    public VIDE_Assign throwGarbageComment;

    public IEnumerator _ThrowGarbage(InteractableObj obj)
    {
        Animator.SetTrigger(openAnimationTrigger);

        bool found = false;
        foreach(DropObject dropObj in dropObjs)
        {
            if(dropObj.obj == obj)
            {
                dropObj.quantity++;
                found = true;
                break;
            }
        }

        if(!found)
        {
            DropObject dropObj = new DropObject();
            dropObj.obj = obj;
            dropObj.banObjs = new List<InteractableObj>();
            dropObj.quantity = 1;

            dropObjs.Add(dropObj);
        }

        DialogueUIController.PrepareDialogueUI(this, throwGarbageComment);
        yield return StartCoroutine(_BeginDialogue(throwGarbageComment));

        Animator.SetTrigger(closeAnimationTrigger);
    }
}