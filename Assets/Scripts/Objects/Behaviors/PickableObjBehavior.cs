using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickableObjBehavior : InteractableObjBehavior
{
    public bool inInventory;

    public override void _GetPicked()
    {
        base._GetPicked();

        inInventory = true;

        PCController.Instance.InventoryController.AddItemToInventory(this);
    }
}
