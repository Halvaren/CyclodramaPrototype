using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterDispenserObjBehavior : InteractableObjBehavior
{
    public GameObject cupGO;

    protected override void InitializeObjBehavior()
    {
        base.InitializeObjBehavior();

        cupGO.SetActive(false);
    }

    public IEnumerator _FillCup(CupObjBehavior cup)
    {
        cupGO.SetActive(true);

        yield return new WaitForSeconds(3f);

        cupGO.SetActive(false);

        PCController.InventoryController.RemoveItemFromInventory(cup.obj);
        if(cup.cut)
        {
            PCController.InventoryController.AddItemToInventory(cup.cutCuptWithWater);
        }
        else
        {
            PCController.InventoryController.AddItemToInventory(cup.cupWithWater);
        }
    }
}
