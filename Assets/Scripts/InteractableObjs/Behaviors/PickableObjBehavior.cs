using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ObjRelationSet
{
    ListOfObjs, AllDoors, AllPickableObjs, AllSubjs, RestOfObjs
}

public class PickableObjBehavior : InteractableObjBehavior
{
    [HideInInspector]
    public bool inventoryObj;
    public PickAnimationWeight objWeight;
    public PickAnimationHeight objHeight;
    public bool characterVisibleToPick;

    [HideInInspector]
    public List<ObjRelation> useObjRelations;
    [HideInInspector]
    public List<ObjRelation> giveObjRelations;
    [HideInInspector]
    public List<ObjRelation> hitObjRelations;
    [HideInInspector]
    public List<ObjRelation> drawObjRelations;
    [HideInInspector]
    public List<ObjRelation> throwObjRelations;

    public override IEnumerator _GetPicked()
    {
        if(characterVisibleToPick)
        {
            AddAnimationLock();
            PCController.instance.animationCallback += ReleaseAnimationLock;
            PCController.instance.AnimationController.PickObject(objHeight, objWeight);

            while (animationLocks.Count > 0)
            {
                yield return null;
            }

            PCController.instance.animationCallback -= ReleaseAnimationLock;
        }
        
        yield return base._GetPicked();
        AddToInventory();
    }

    void AddToInventory()
    {
        PCController.instance.InventoryController.AddItemToInventory(this);
    }

    public override bool CheckUseOfVerb(ActionVerb verb, bool ignoreWalk = true)
    {
        if (inventoryObj && verb == DataManager.Instance.verbsDictionary["pick"]) return false;
        return base.CheckUseOfVerb(verb, ignoreWalk);
    }

    protected int GetObjRelationIndex(InteractableObjBehavior targetObj, List<ObjRelation> objRelations)
    {
        int restOfObjectsIndex = -1;

        foreach (ObjRelation useObjRelation in objRelations)
        {
            if ((useObjRelation.objSet == ObjRelationSet.AllPickableObjs && targetObj is PickableObjBehavior) ||
                (useObjRelation.objSet == ObjRelationSet.AllDoors && targetObj is DoorBehavior) ||
                (useObjRelation.objSet == ObjRelationSet.AllSubjs && targetObj is NPCBehavior))
            {
                return useObjRelation.index;
            }
            else if (useObjRelation.objSet == ObjRelationSet.RestOfObjs)
            {
                restOfObjectsIndex = useObjRelation.index;
            }
            else if (useObjRelation.objSet == ObjRelationSet.ListOfObjs)
            {
                foreach (InteractableObj obj in useObjRelation.objs)
                {
                    if (obj == targetObj.obj)
                        return useObjRelation.index;
                }
            }
        }

        return restOfObjectsIndex;
    }

    public virtual IEnumerator UseMethod(InteractableObjBehavior targetObj)
    {
        yield return null;
    }

    public virtual IEnumerator GiveMethod(InteractableObjBehavior targetObj)
    {
        yield return null;
    }

    public virtual IEnumerator HitMethod(InteractableObjBehavior targetObj)
    {
        yield return null;
    }

    public virtual IEnumerator DrawMethod(InteractableObjBehavior targetObj)
    {
        yield return null;
    }

    public virtual IEnumerator ThrowMethod(InteractableObjBehavior targetObj)
    {
        yield return null;
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
public class ObjRelation
{
    public int index;
    public List<InteractableObj> objs;

    public ObjRelationSet objSet;
}
