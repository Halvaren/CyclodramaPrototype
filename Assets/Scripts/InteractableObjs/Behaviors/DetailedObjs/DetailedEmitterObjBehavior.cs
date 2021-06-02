using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetailedEmitterObjBehavior : DetailedObjBehavior
{
    public List<DropObject> dropObjs;

    public VIDE_Assign emptyComment;
    public VIDE_Assign dropObjsComment;
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

                foreach (DropObject droppedObj in droppedObjs)
                {
                    droppedObj.quantity--;
                    InventoryController.AddItemToInventory(droppedObj.obj);
                }
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

        if(data is DetailedEmitterObjData detailedEmitterObjData)
        {
            dropObjs = new List<DropObject>();

            foreach(DropObjData dropObj in detailedEmitterObjData.dropObjs)
            {
                dropObjs.Add(new DropObject(dropObj));
            }
        }
    }

    public override InteractableObjData GetObjData()
    {
        return new DetailedEmitterObjData(inScene, dropObjs);
    }

    #endregion
}
