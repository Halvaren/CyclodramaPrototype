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
    public bool inInventory;

    [HideInInspector]
    public List<UseReaction> useReactions;

    public override void _GetPicked()
    {
        base._GetPicked();

        inInventory = true;

        PCController.Instance.InventoryController.AddItemToInventory(this);
    }

    public override bool _CheckUseOfVerb(ActionVerb verb, bool ignoreWalk = true)
    {
        if (inInventory && verb == PCController.Instance.ActionController.pick) return false;
        return base._CheckUseOfVerb(verb, ignoreWalk);
    }

    public virtual int UseMethod(InteractableObjBehavior targetObj)
    {
        int restOfObjectsIndex = -1;

        foreach(UseReaction useReaction in useReactions)
        {
            if((useReaction.objSet == UseReactionObjSet.AllPickableObjs && targetObj is PickableObjBehavior) ||
                (useReaction.objSet == UseReactionObjSet.AllDoors && targetObj is DoorBehavior) ||
                (useReaction.objSet == UseReactionObjSet.AllSubjs && targetObj is NPCController))
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
}

[System.Serializable]
public class UseReaction
{
    public int index;
    public List<InteractableObj> objs;

    public UseReactionObjSet objSet;

    public UseReaction()
    {

    }
}
