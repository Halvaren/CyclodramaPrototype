using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenableLockedEmitterObjBehavior : OpenableEmitterObjBehavior
{
    public bool locked;

    public VIDE_Assign lockedComment;
    public VIDE_Assign nowUnlockedComment;
    public VIDE_Assign alreadyUnlockedComment;

    //Open method
    public override IEnumerator DropObjs()
    {
        if (locked)
        {
            DialogueUIController.PrepareDialogueUI(this, lockedComment);
            yield return StartCoroutine(_BeginDialogue(lockedComment));
        }
        else
        {
            yield return StartCoroutine(base.DropObjs());
        }
    }

    //Force lock method
    public IEnumerator Unlock()
    {
        if(locked)
        {
            DialogueUIController.PrepareDialogueUI(this, nowUnlockedComment);
            yield return StartCoroutine(_BeginDialogue(nowUnlockedComment));

            locked = false;
        }
        else
        {
            DialogueUIController.PrepareDialogueUI(this, alreadyUnlockedComment);
            yield return StartCoroutine(_BeginDialogue(alreadyUnlockedComment));
        }
    }
}