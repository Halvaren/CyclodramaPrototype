using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UseReactionObjSet
{
    ListOfObjs, AllDoors, AllPickableObjs, AllSubjs, RestOfObjs
}

public class PickableObjBehavior : InteractableObjBehavior
{
    [HideInInspector]
    public bool inventoryObj;

    [HideInInspector]
    public List<UseReaction> useReactions;

    public override void _GetPicked()
    {
        base._GetPicked();

        AddToInventory();
    }

    void AddToInventory()
    {
        PCController.Instance.InventoryController.AddItemToInventory(this);
    }

    public override bool _CheckUseOfVerb(ActionVerb verb, bool ignoreWalk = true)
    {
        if (inventoryObj && verb == PCController.Instance.ActionController.pick) return false;
        return base._CheckUseOfVerb(verb, ignoreWalk);
    }

    public virtual int UseMethod(InteractableObjBehavior targetObj)
    {
        int restOfObjectsIndex = -1;

        foreach(UseReaction useReaction in useReactions)
        {
            if((useReaction.objSet == UseReactionObjSet.AllPickableObjs && targetObj is PickableObjBehavior) ||
                (useReaction.objSet == UseReactionObjSet.AllDoors && targetObj is DoorBehavior) ||
                (useReaction.objSet == UseReactionObjSet.AllSubjs && targetObj is NPCBehavior))
            {
                return useReaction.index;
            }
            else if(useReaction.objSet == UseReactionObjSet.RestOfObjs)
            {
                restOfObjectsIndex = useReaction.index;
            }
            else if(useReaction.objSet == UseReactionObjSet.ListOfObjs)
            {
                foreach(InteractableObj obj in useReaction.objs)
                {
                    if (obj == targetObj.obj)
                        return useReaction.index;
                }
            }
        }

        return restOfObjectsIndex;
    }

    #region Data methods

    public void _LoadData(PickableObjData data)
    {
        _ApplyData(data.inScene, data.inventoryObj);
    }

    public void _ApplyData(bool inScene, bool inInventory)
    {
        _ApplyData(inScene);

        this.inventoryObj = inInventory;
    }

    public override InteractableObjData _GetObjData()
    {
        return new PickableObjData(inScene, inventoryObj);
    }

    #endregion
}

[System.Serializable]
public class UseReaction
{
    public int index;
    public List<InteractableObj> objs;

    public UseReactionObjSet objSet;
}
