using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ObjRelationSet
{
    ListOfObjs, AllDoors, AllPickableObjs, AllSubjs, RestOfObjs
}

/// <summary>
/// Manages the behavior of an object that can be picked or stolen
/// </summary>
public class PickableObjBehavior : InteractableObjBehavior
{
    [HideInInspector]
    public bool inventoryObj;

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

    /// <summary>
    /// Executed when player picks up the object
    /// </summary>
    /// <returns></returns>
    public override IEnumerator _GetPicked()
    {
        yield return base._GetPicked();
        AddToInventory();
    }

    /// <summary>
    /// Executed when player steals the object
    /// </summary>
    /// <returns></returns>
    public override IEnumerator _GetStolen()
    {
        yield return base._GetStolen();
        AddToInventory();
    }

    /// <summary>
    /// Adds the object to the inventory
    /// </summary>
    void AddToInventory()
    {
        PCController.InventoryController.AddItemToInventory(new List<InteractableObj> { obj });
    }

    /// <summary>
    /// Checks if the verb want to use with the object has an available reaction (useOfVerb)
    /// </summary>
    /// <param name="verb"></param>
    /// <param name="ignoreWalk"></param>
    /// <returns></returns>
    public override bool CheckUseOfVerb(ActionVerb verb, bool ignoreWalk = true)
    {
        //If the object is an inventory item and the selected verb is pick or steal, it can't be interacted
        if (inventoryObj && (verb == DataManager.Instance.verbsDictionary["pick"] || verb == DataManager.Instance.verbsDictionary["steal"])) return false;
        return base.CheckUseOfVerb(verb, ignoreWalk);
    }

    /// <summary>
    /// Given a second interactable object and a list of object relations, it returns the index of the objRelation where it belongs the second interactable object
    /// </summary>
    /// <param name="targetObj"></param>
    /// <param name="objRelations"></param>
    /// <returns></returns>
    protected int GetObjRelationIndex(InteractableObjBehavior targetObj, List<ObjRelation> objRelations)
    {
        int restOfObjectsIndex = -1;

        foreach (ObjRelation useObjRelation in objRelations)
        {
            if ((useObjRelation.objSet == ObjRelationSet.AllPickableObjs && targetObj is PickableObjBehavior) ||
                (useObjRelation.objSet == ObjRelationSet.AllDoors && targetObj is SetDoorBehavior) ||
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

    /// <summary>
    /// Executed when player uses Use verb with the object
    /// </summary>
    /// <param name="targetObj"></param>
    /// <returns></returns>
    public virtual IEnumerator UseMethod(InteractableObjBehavior targetObj)
    {
        int index = GetObjRelationIndex(targetObj, useObjRelations);

        if (index == -1)
        {
            Debug.Log("Error");
        }

        if (index == 0)
        {
            yield return StartCoroutine(_StartConversation(defaultUseComment));
        }

        if(index == 9)
        {
            TrashCanEmitterObjBehavior trashCan = (TrashCanEmitterObjBehavior)targetObj;

            yield return StartCoroutine(trashCan._ThrowGarbage(this));

            PCController.InventoryController.RemoveItemFromInventory(obj);
        }
    }

    /// <summary>
    /// Executed when player uses Give verb with the object
    /// </summary>
    /// <param name="targetObj"></param>
    /// <returns></returns>
    public virtual IEnumerator GiveMethod(InteractableObjBehavior targetObj)
    {
        int index = GetObjRelationIndex(targetObj, giveObjRelations);

        if (index == -1)
        {
            Debug.Log("Error");
        }

        if (index == 0)
        {
            yield return StartCoroutine(_StartConversation(defaultGiveComment));
        }
    }

    /// <summary>
    /// Executed when player uses Hit verb with the object
    /// </summary>
    /// <param name="targetObj"></param>
    /// <returns></returns>
    public virtual IEnumerator HitMethod(InteractableObjBehavior targetObj)
    {
        int index = GetObjRelationIndex(targetObj, hitObjRelations);

        if (index == -1)
        {
            Debug.Log("Error");
        }
        
        if (index == 0)
        {
            yield return StartCoroutine(_StartConversation(defaultHitComment));
        }
    }

    /// <summary>
    /// Executed when player uses Draw verb with the object
    /// </summary>
    /// <param name="targetObj"></param>
    /// <returns></returns>
    public virtual IEnumerator DrawMethod(InteractableObjBehavior targetObj)
    {
        int index = GetObjRelationIndex(targetObj, drawObjRelations);

        if (index == -1)
        {
            Debug.Log("Error");
        }
        
        if (index == 0)
        {
            yield return StartCoroutine(_StartConversation(defaultDrawComment));
        }
    }

    /// <summary>
    /// Executed when player uses Throw verb with the object
    /// </summary>
    /// <param name="targetObj"></param>
    /// <returns></returns>
    public virtual IEnumerator ThrowMethod(InteractableObjBehavior targetObj)
    {
        int index = GetObjRelationIndex(targetObj, throwObjRelations);

        if (index == -1)
        {
            Debug.Log("Error");
        }
        
        if (index == 0)
        {
            yield return StartCoroutine(_StartConversation(defaultThrowComment));
        }
    }

    #region Data methods

    /// <summary>
    /// Loads the data received as a parameter in the variables
    /// </summary>
    /// <param name="data"></param>
    public override void LoadData(InteractableObjData data)
    {
        base.LoadData(data);

        if(data is PickableObjData pickableObjData)
        {
            inventoryObj = pickableObjData.inventoryObj;
        }
    }

    /// <summary>
    /// Returns a data object with the info of the behavior
    /// </summary>
    /// <returns></returns>
    public override InteractableObjData GetObjData()
    {
        return new PickableObjData(inScene, inventoryObj);
    }

    #endregion
}

/// <summary>
/// An object relation define a relation between a main object and a group of objects, identified with an index. The group of objects could be a list of specific objects or a set of objects 
/// that meet certain conditions (all subjects, all pickable objects, rest of objects...)
/// </summary>
[System.Serializable]
public class ObjRelation
{
    public int index;
    public List<InteractableObj> objs;

    public ObjRelationSet objSet;
}
