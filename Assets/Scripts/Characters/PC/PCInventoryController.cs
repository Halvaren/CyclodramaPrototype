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
    [HideInInspector]
    public PickableObjBehavior pointedObj;

    public CameraManager CameraManager
    {
        get { return m_PCController.CameraManager; }
    }

    public GeneralUIController GeneralUIController
    {
        get { return m_PCController.GeneralUIController; }
    }

    public InventoryUIController InventoryUIController
    {
        get { return m_PCController.InventoryUIController; }
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

        InventoryUIController.InitializeInventoryUI(objBehaviorsInInventory);
    }

    public void AddItemToInventory(PickableObjBehavior objBehavior)
    {
        objBehavior.transform.parent = InventoryGO.transform;
        objBehaviorsInInventory.Add(objBehavior);

        InventoryUIController.AddObjCell(objBehavior);
    }

    public void OpenInventory()
    {
        GeneralUIController.DisplayInventoryUI();
        CameraManager.LockUnlockCurrentDetailCamera(false);
    }

    public void CloseInventory()
    {
        GeneralUIController.DisplayGameplayUI();
        CameraManager.LockUnlockCurrentDetailCamera(true);
    }
}
