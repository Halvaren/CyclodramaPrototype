using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the behavior of an object that contains other objects and drops them when player interacts with the emitter object
/// </summary>
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

    /// <summary>
    /// Executed when player picks up the emitter object
    /// </summary>
    /// <returns></returns>
    public override IEnumerator _GetPicked()
    {
        yield return StartCoroutine(DropObjs(PlayPickAnimation()));
    }

    /// <summary>
    /// Executed when player steals the emitter obejct
    /// </summary>
    /// <returns></returns>
    public override IEnumerator _GetStolen()
    {
        yield return StartCoroutine(DropObjs(PlayStealAnimation()));
    }

    /// <summary>
    /// Adds to the inventory the objects inside the emitter that aren't already in the inventory
    /// </summary>
    /// <param name="animationMethod"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Returns the list of objects to add to the inventory
    /// </summary>
    /// <param name="theresSomething"></param>
    /// <param name="pickSomething"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Loads the data received as a parameter in the variables
    /// </summary>
    /// <param name="data"></param>
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

    /// <summary>
    /// Returns a data object with the info of the behavior
    /// </summary>
    /// <returns></returns>
    public override InteractableObjData GetObjData()
    {
        return new EmitterObjData(inScene, dropObjs);
    }

    #endregion
}

/// <summary>
/// Represents an object or group of objects that will drop from an emitter object when player interacts with it. It has a list of ban objects that determines which objects in the inventory will
/// prevent the object to be dropped
/// </summary>
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
