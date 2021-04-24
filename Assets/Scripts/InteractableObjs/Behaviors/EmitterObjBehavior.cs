using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmitterObjBehavior : InteractableObjBehavior
{
    public List<int> quantityPerObj;
    public List<InteractableObj> objsToDrop;

    public virtual List<InteractableObj> DropObjs()
    {
        return objsToDrop;
    }

    #region Data methods

    public void _LoadData(EmitterObjData data)
    {
        _ApplyData(data.inScene, data.quantityPerObj, data.objToDropIDs);
    }

    public void _ApplyData(bool inScene, List<int> quantityPerObj, List<int> objToDropIDs)
    {
        _ApplyData(inScene);

        this.quantityPerObj = new List<int>();
        objsToDrop = new List<InteractableObj>();

        foreach(int quantity in quantityPerObj)
        {
            this.quantityPerObj.Add(quantity);
        }
        
        /*foreach(InteractableObj obj in objsToDrop)
        {
            this.objsToDrop.Add(obj);
        }*/
    }

    public override InteractableObjData _GetObjData()
    {
        return new EmitterObjData(inScene, quantityPerObj, objsToDrop);
    }

    #endregion
}
