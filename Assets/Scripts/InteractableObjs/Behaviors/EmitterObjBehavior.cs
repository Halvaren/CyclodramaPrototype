using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmitterObjBehavior : InteractableObjBehavior
{
    [HideInInspector]
    public List<DropObject> dropObjs;

    PCInventoryController InventoryController
    {
        get
        {
            return PCController.Instance.InventoryController;
        }
    }

    public virtual void DropObjs()
    {
        foreach(DropObject dropObj in dropObjs)
        {
            if(dropObj.quantity != 0)
            {
                bool canPick = true;
                foreach(InteractableObj banObj in dropObj.banObjs)
                {
                    if(InventoryController.IsItemInInventory(banObj))
                    {
                        canPick = false;
                        break;
                    }
                }

                if (canPick)
                {
                    InventoryController.AddItemToInventory(dropObj.obj);
                    dropObj.quantity--;
                }
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
