using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FabricColor
{
    Red, Pink, Purple, NavyBlue, LightBlue, Green, GreenishYellow, Yellow, Orange, White, Black, Grey
}

public class FabricObjBehavior : PickableObjBehavior
{
    public FabricColor color;
    public bool inspected = false;

    public VIDE_Assign lookComment;
    public VIDE_Assign inspectComment;
    public VIDE_Assign throwToGarbageComment;

    public override IEnumerator GiveMethod(InteractableObjBehavior targetObj)
    {
        int index = GetObjRelationIndex(targetObj, giveObjRelations);

        //Belinda
        if(index == 1)
        {
            BelindaBehavior belinda = (BelindaBehavior)targetObj;
            yield return belinda.StartCoroutine(belinda._GiveObj(obj));
        }

        yield return base.GiveMethod(targetObj);
    }

    public override IEnumerator UseMethod(InteractableObjBehavior targetObj)
    {
        int index = GetObjRelationIndex(targetObj, useObjRelations);

        if(index == 9)
        {
            yield return StartCoroutine(_StartConversation(throwToGarbageComment));
        }
        else
        {
            yield return base.UseMethod(targetObj);
        }
    }

    public IEnumerator LookMethod()
    {
        if(!inspected)
        {
            yield return StartCoroutine(_StartConversation(lookComment));
        }
        else
        {
            yield return StartCoroutine(_StartConversation(inspectComment));
        }
    }

    public IEnumerator InspectMethod()
    {
        yield return StartCoroutine(_StartConversation(inspectComment));

        inspected = true;
    }

    public override string GetObjName()
    {
        if (inspected && obj is FabricObj fabricObj)
            return fabricObj.realName;
        return obj.name;
    }

    public override Sprite GetInventorySprite()
    {
        if (obj is FabricObj fabricObj)
            return fabricObj.GetInventorySprite(color);
        return obj.GetInventorySprite();
    }

    public override void LoadData(InteractableObjData data)
    {
        base.LoadData(data);

        if(data is FabricObjData fabricObjData)
        {
            color = fabricObjData.color;
            inspected = fabricObjData.inspected;
        }
    }

    public override InteractableObjData GetObjData()
    {
        return new FabricObjData(inScene, inventoryObj, color, inspected);
    }
}
