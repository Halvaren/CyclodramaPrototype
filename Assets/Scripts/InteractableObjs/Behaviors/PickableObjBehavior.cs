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
    [HideInInspector]
    public PickAnimationWeight objWeight;
    [HideInInspector]
    public PickAnimationHeight objHeight;
    [HideInInspector]
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

    [HideInInspector]
    public VIDE_Assign defaultUseComment;
    [HideInInspector]
    public VIDE_Assign defaultDrawComment;
    [HideInInspector]
    public VIDE_Assign defaultGiveComment;
    [HideInInspector]
    public VIDE_Assign defaultHitComment;
    [HideInInspector]
    public VIDE_Assign defaultThrowComment;
    [HideInInspector]
    public VIDE_Assign cannotPickComment;

    public override IEnumerator _GetPicked()
    {
        if(characterVisibleToPick)
        {
            AddAnimationLock();
            PCController.instance.mainAnimationCallback += ReleaseAnimationLock;
            PCController.instance.AnimationController.PickObject(objHeight, objWeight);

            while (animationLocks.Count > 0)
            {
                yield return null;
            }

            PCController.instance.mainAnimationCallback -= ReleaseAnimationLock;
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
        int index = GetObjRelationIndex(targetObj, useObjRelations);

        if (index == -1)
        {
            Debug.Log("Error");
        }

        if (index == 0)
        {
            DialogueUIController.PrepareDialogueUI(this, defaultUseComment);
            yield return StartCoroutine(_BeginDialogue(defaultUseComment));
        }

        if(index == 9)
        {
            TrashCanEmitterObjBehavior trashCan = (TrashCanEmitterObjBehavior)targetObj;

            yield return StartCoroutine(trashCan._ThrowGarbage(obj));

            PCController.InventoryController.RemoveItemFromInventory(obj);
        }
    }

    public virtual IEnumerator GiveMethod(InteractableObjBehavior targetObj)
    {
        int index = GetObjRelationIndex(targetObj, giveObjRelations);

        if (index == -1)
        {
            Debug.Log("Error");
        }

        if (index == 0)
        {
            DialogueUIController.PrepareDialogueUI(this, defaultGiveComment);
            yield return StartCoroutine(_BeginDialogue(defaultGiveComment));
        }
    }

    public virtual IEnumerator HitMethod(InteractableObjBehavior targetObj)
    {
        int index = GetObjRelationIndex(targetObj, hitObjRelations);

        if (index == -1)
        {
            Debug.Log("Error");
        }
        
        if (index == 0)
        {
            DialogueUIController.PrepareDialogueUI(this, defaultHitComment);
            yield return StartCoroutine(_BeginDialogue(defaultHitComment));
        }
    }

    public virtual IEnumerator DrawMethod(InteractableObjBehavior targetObj)
    {
        int index = GetObjRelationIndex(targetObj, drawObjRelations);

        if (index == -1)
        {
            Debug.Log("Error");
        }
        
        if (index == 0)
        {
            DialogueUIController.PrepareDialogueUI(this, defaultDrawComment);
            yield return StartCoroutine(_BeginDialogue(defaultDrawComment));
        }
    }

    public virtual IEnumerator ThrowMethod(InteractableObjBehavior targetObj)
    {
        int index = GetObjRelationIndex(targetObj, throwObjRelations);

        if (index == -1)
        {
            Debug.Log("Error");
        }
        
        if (index == 0)
        {
            DialogueUIController.PrepareDialogueUI(this, defaultThrowComment);
            yield return StartCoroutine(_BeginDialogue(defaultThrowComment));
        }
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
