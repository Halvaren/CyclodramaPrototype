using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShovelObjBehavior : PickableObjBehavior
{
    public VIDE_Assign pickConversation;

    public override IEnumerator _GetPicked()
    {
        DialogueUIController.PrepareDialogueUI(this, pickConversation);
        yield return StartCoroutine(_BeginDialogue(pickConversation));

        yield return base._GetPicked();
    }
}
