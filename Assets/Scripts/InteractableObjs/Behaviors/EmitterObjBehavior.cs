using System;
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
                    yield return StartCoroutine(_StartConversation(dropObjsComment));
                }

                if (characterVisibleToPick)
                {
                    yield return StartCoroutine(animationMethod);
                }

                List<InteractableObj> objsToAdd = new List<InteractableObj>();

                foreach (DropObject droppedObj in droppedObjs)
                {
                    droppedObj.quantity--;
                    objsToAdd.Add(droppedObj.obj);
                }

                InventoryController.AddItemToInventory(objsToAdd);
            }
            else if (!pickSomething && haveEnoughComment != null)
            {
                yield return StartCoroutine(_StartConversation(haveEnoughComment));
            }
        }
        else
        {
            if (emptyComment != null)
            {
                yield return StartCoroutine(_StartConversation(emptyComment));
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

    public override void LoadData(InteractableObjData data)
    {
        base.LoadData(data);

        if(data is EmitterObjData emitterObjData)
        {
            dropObjs = new List<DropObject>();

            foreach (DropObjData dropObj in emitterObjData.dropObjs)
            {
                dropObjs.Add(new DropObject(dropObj));
            }
        }
    }

    public override InteractableObjData GetObjData()
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

    public DropObject(DropObjData data)
    {
        quantity = data.quantity;
        obj = DataManager.Instance.pickableObjsDictionary[data.objID];

        banObjs = new List<InteractableObj>();
        foreach(int objID in data.banObjsIDs)
        {
            banObjs.Add(DataManager.Instance.pickableObjsDictionary[objID]);
        }
    }
}
