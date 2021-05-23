using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetailedEmitterObjBehavior : DetailedObjBehavior
{
    public List<DropObject> dropObjs;

    public VIDE_Assign emptyComment;
    public VIDE_Assign dropObjsComment;
    public VIDE_Assign haveEnoughComment;

    protected PCInventoryController InventoryController
    {
        get
        {
            return PCController.instance.InventoryController;
        }
    }

    public override IEnumerator _GetPicked()
    {
        yield return StartCoroutine(DropObjs(PlayPickAnimation()));
    }

    public override IEnumerator _GetStolen()
    {
        yield return StartCoroutine(DropObjs(PlayStealAnimation()));
    }

    protected virtual IEnumerator DropObjs(IEnumerator animationMethod)
    {
        bool theresSomething = false;
        bool pickSomething = false;

        List<DropObject> droppedObjs = GetDropObjects(ref theresSomething, ref pickSomething);

        if (theresSomething)
        {
            if (pickSomething)
            {
                if(dropObjsComment != null)
                {
                    DialogueUIController.PrepareDialogueUI(this, dropObjsComment);
                    yield return StartCoroutine(_BeginDialogue(dropObjsComment));
                }

                if (characterVisibleToPick)
                {
                    yield return StartCoroutine(animationMethod);
                }

                foreach (DropObject droppedObj in droppedObjs)
                {
                    droppedObj.quantity--;
                    InventoryController.AddItemToInventory(droppedObj.obj);
                }
            }
            else if (!pickSomething && haveEnoughComment != null)
            {
                DialogueUIController.PrepareDialogueUI(this, haveEnoughComment);
                yield return StartCoroutine(_BeginDialogue(haveEnoughComment));
            }
        }
        else
        {
            if (emptyComment != null)
            {
                DialogueUIController.PrepareDialogueUI(this, emptyComment);
                yield return StartCoroutine(_BeginDialogue(emptyComment));
            }
        }
    }

    protected virtual List<DropObject> GetDropObjects(ref bool theresSomething, ref bool pickSomething)
    {
        List<DropObject> result = new List<DropObject>();
        foreach (DropObject dropObj in dropObjs)
        {
            if (dropObj.quantity != 0)
            {
                theresSomething = true;
                bool canPick = true;
                foreach (InteractableObj banObj in dropObj.banObjs)
                {
                    if (InventoryController.IsItemInInventory(banObj))
                    {
                        canPick = false;
                        break;
                    }
                }

                if (canPick)
                {
                    pickSomething = true;
                    result.Add(dropObj);
                }
            }
        }

        return result;
    }

    #region Data methods

    /*public void _LoadData(EmitterObjData data)
    {
        _ApplyData(data.inScene, data.dropObjs);
    }

    public void _ApplyData(bool inScene, List<DropObject> dropObjs)
    {
        _ApplyData(inScene);

        this.dropObjs = new List<DropObject>();

        foreach (DropObject dropObj in dropObjs)
        {
            this.dropObjs.Add(dropObj);
        }
    }

    public override InteractableObjData _GetObjData()
    {
        return new EmitterObjData(inScene, dropObjs);
    }*/

    #endregion
}
