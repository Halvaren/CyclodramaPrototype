using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FabricColor
{
    Red, Pink, Purple, NavyBlue, LightBlue, Green, GreenishYellow, Yellow, Orange, White, Black, Grey
}

public class FabricObjBehavior : PickableObjBehavior
{
    public FabricColor color;
    public bool inspected = false;

    public VIDE_Assign lookComment;
    public VIDE_Assign inspectComment;

    public override IEnumerator GiveMethod(InteractableObjBehavior targetObj)
    {
        int index = GetObjRelationIndex(targetObj, giveObjRelations);

        yield return base.GiveMethod(targetObj);
    }

    public IEnumerator LookMethod()
    {
        if(!inspected)
        {
            DialogueUIController.PrepareDialogueUI(this, lookComment);
            yield return StartCoroutine(_BeginDialogue(lookComment));
        }
        else
        {
            DialogueUIController.PrepareDialogueUI(this, inspectComment);
            yield return StartCoroutine(_BeginDialogue(inspectComment));
        }
    }

    public IEnumerator InspectMethod()
    {
        DialogueUIController.PrepareDialogueUI(this, inspectComment);
        yield return StartCoroutine(_BeginDialogue(inspectComment));

        inspected = true;
    }

    public override string GetObjName()
    {
        if (inspected && obj is FabricObj fabricObj)
            return fabricObj.realName;
        return obj.name;
    }

    public override Sprite GetInventorySprite()
    {
        if (obj is FabricObj fabricObj)
            return fabricObj.GetInventorySprite(color);
        return obj.GetInventorySprite();
    }
}
