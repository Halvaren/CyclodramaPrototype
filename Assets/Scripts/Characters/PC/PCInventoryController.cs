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
    InventoryData inventoryData;

    [HideInInspector]
    public PickableObjBehavior pointedObj;

    public CameraManager CameraManager { get { return m_PCController.CameraManager; } }

    public GeneralUIController GeneralUIController { get { return m_PCController.GeneralUIController; } }

    public InventoryUIController InventoryUIController { get { return m_PCController.InventoryUIController; } }

    public DataManager DataManager { get { return DataManager.instance; } }

    public void InitializeInventory()
    {
        GetInventoryObjs();
        inventoryData = DataManager.GetInvenetoryData();

        if(inventoryData == null)
        {
            SaveInventoryData();
        }
        else
        {
            LoadInventoryData();
        }

        DataManager.OnSaveData += SaveInventoryData;
        InventoryUIController.InitializeInventoryUI(objBehaviorsInInventory);
    }

    void GetInventoryObjs()
    {
        PickableObjBehavior[] objBehaviors = GetComponentsInChildren<PickableObjBehavior>();

        objBehaviorsInInventory = new List<PickableObjBehavior>();
        foreach(PickableObjBehavior behavior in objBehaviors)
        {
            objBehaviorsInInventory.Add(behavior);
        }
    }

    public void LoadInventoryData()
    {
        foreach (InteractableObjBehavior behavior in objBehaviorsInInventory)
        {
            if (behavior.obj != null)
            {
                if (inventoryData.pickableObjInInventoryDatas.ContainsKey(behavior.obj.objID))
                {
                    behavior._LoadData(inventoryData.pickableObjInInventoryDatas[behavior.obj.objID]);
                }
            }
        }
    }

    public void SaveInventoryData()
    {
        if(inventoryData == null)
        {
            inventoryData = new InventoryData();
        }

        foreach(PickableObjBehavior behavior in objBehaviorsInInventory)
        {
            if(behavior.obj != null)
            {
                PickableObjData objData = (PickableObjData)behavior._GetObjData();
                if (inventoryData.pickableObjInInventoryDatas.ContainsKey(behavior.obj.objID))
                    inventoryData.pickableObjInInventoryDatas[behavior.obj.objID] = objData;
                else
                    inventoryData.pickableObjInInventoryDatas.Add(behavior.obj.objID, objData);
            }
        }

        DataManager.SetInventoryData(inventoryData);
    }

    public void AddItemToInventory(PickableObjBehavior objBehavior)
    {
        foreach(PickableObjBehavior objBehaviorInInventory in objBehaviorsInInventory)
        {
            if(objBehavior == objBehaviorInInventory)
            {
                objBehaviorInInventory.gameObject.SetActive(true);
                break;
            }
        }

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
