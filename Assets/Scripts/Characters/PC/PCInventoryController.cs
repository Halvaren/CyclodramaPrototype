using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PCComponents/Inventory Controller")]
public class PCInventoryController : PCComponent
{
    private GameObject inventoryGO;
    public GameObject InventoryGO
    {
        get
        {
            if (inventoryGO == null) inventoryGO = m_PCController.inventoryGO;

            return inventoryGO;
        }
    }

    public List<PickableObjBehavior> objBehaviorsInInventory;

    public InventoryUIController inventoryUIController
    {
        get
        {
            return m_PCController.inventoryUIController;
        }
    }

    internal void InitializeInventory()
    {
        objBehaviorsInInventory = new List<PickableObjBehavior>();
        for(int i = 0; i < InventoryGO.transform.childCount; i++)
        {
            Transform child = InventoryGO.transform.GetChild(i);
            PickableObjBehavior objBehavior = child.GetComponent<PickableObjBehavior>();

            if (objBehavior != null) objBehaviorsInInventory.Add(objBehavior);
        }

        inventoryUIController.InitializeInventoryUI(objBehaviorsInInventory.Count);
    }

    public void AddItemToInventory(PickableObjBehavior objBehavior)
    {
        objBehavior.transform.parent = InventoryGO.transform;
        objBehaviorsInInventory.Add(objBehavior);

        inventoryUIController.AddObjCell();
    }

    public void InventoryItemClicked(int index)
    {
        PickableObjBehavior objBehavior = objBehaviorsInInventory[index];

        UseOfVerb useOfVerb = objBehavior._GetUseOfVerb(m_PCController.ActionController.GetSelectedVerb());

        if (useOfVerb != null)
        {
            m_PCController.ManageUseOfVerb(useOfVerb, objBehavior, true);
            return;
        }
    }
}
