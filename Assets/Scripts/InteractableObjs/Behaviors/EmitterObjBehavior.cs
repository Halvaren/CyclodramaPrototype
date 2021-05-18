using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmitterObjBehavior : InteractableObjBehavior
{
    [HideInInspector]
    public List<DropObject> dropObjs;

    [HideInInspector]
    public VIDE_Assign emptyComment;
    [HideInInspector]
    public VIDE_Assign dropObjsComment;
    [HideInInspector]
    public VIDE_Assign haveEnoughComment;

    protected PCInventoryController InventoryController
    {
        get
        {
            return PCController.instance.InventoryController;
        }
    }

    public virtual IEnumerator DropObjs()
    {
        bool theresSomething = false;
        bool pickSomething = false;

        AddObjsToInventory(ref theresSomething, ref pickSomething);

        yield return StartCoroutine(DropObjsComment(theresSomething, pickSomething));
    }

    protected void AddObjsToInventory(ref bool theresSomething, ref bool pickSomething)
    {
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
                    InventoryController.AddItemToInventory(dropObj.obj);
                    dropObj.quantity--;
                }
            }
        }
    }

    protected IEnumerator DropObjsComment(bool theresSomething, bool pickSomething)
    {
        if (theresSomething)
        {
            if (pickSomething && dropObjsComment != null)
            {
                DialogueUIController.PrepareDialogueUI(this, dropObjsComment);
                yield return StartCoroutine(_BeginDialogue(dropObjsComment));
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

    #region Data methods

    public void _LoadData(EmitterObjData data)
    {
        _ApplyData(data.inScene, data.dropObjs);
    }

    public void _ApplyData(bool inScene, List<DropObject> dropObjs)
    {
        _ApplyData(inScene);

        this.dropObjs = new List<DropObject>();
        
        foreach(DropObject dropObj in dropObjs)
        {
            this.dropObjs.Add(dropObj);
        }
    }

    public override InteractableObjData _GetObjData()
    {
        return new EmitterObjData(inScene, dropObjs);
    }

    #endregion
}

[System.Serializable]
public class DropObject
{
    public int quantity;
    public InteractableObj obj;
    public List<InteractableObj> banObjs;

    public DropObject()
    {

    }

    public DropObject(DropObject other)
    {
        quantity = other.quantity;
        obj = other.obj;

        banObjs = new List<InteractableObj>();
        foreach(InteractableObj obj in other.banObjs)
        {
            banObjs.Add(obj);
        }
    }
}
