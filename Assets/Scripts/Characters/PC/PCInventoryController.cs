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

    public List<PickableObjBehavior> objBehaviorsInInventory
    {
        get { return m_PCController.objBehaviorsInInventory; }
        set { m_PCController.objBehaviorsInInventory = value; }
    }
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
        PickableObjBehavior[] objBehaviors = GetComponentsInChildren<PickableObjBehavior>(true);

        objBehaviorsInInventory = new List<PickableObjBehavior>();
        foreach(PickableObjBehavior behavior in objBehaviors)
        {
            objBehaviorsInInventory.Add(behavior);
        }
    }

    public void LoadInventoryData()
    {
        foreach (PickableObjBehavior behavior in objBehaviorsInInventory)
        {
            if (behavior.obj != null)
            {
                if (inventoryData.pickableObjInInventoryDatas.ContainsKey(behavior.obj.objID))
                {
                    PickableObjData pickableObjData = inventoryData.pickableObjInInventoryDatas[behavior.obj.objID];

                    bool aux = pickableObjData.inventoryObj;
                    pickableObjData.inventoryObj = pickableObjData.inScene;
                    pickableObjData.inScene = aux;

                    behavior._LoadData(pickableObjData);
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
            if(objBehavior.obj == objBehaviorInInventory.obj)
            {
                objBehaviorInInventory.gameObject.SetActive(true);
                objBehaviorInInventory.inScene = true;

                InventoryUIController.AddObjCell(objBehaviorInInventory);
                break;
            }
        }
    }

    public void AddItemToInventory(InteractableObj obj)
    {
        foreach(PickableObjBehavior objBehaviorInInventory in objBehaviorsInInventory)
        {
            if(objBehaviorInInventory.obj == obj)
            {
                objBehaviorInInventory.gameObject.SetActive(true);
                objBehaviorInInventory.inScene = true;

                InventoryUIController.AddObjCell(objBehaviorInInventory);
                break;
            }
        }
    }

    public bool IsItemInInventory(InteractableObj obj)
    {
        foreach(PickableObjBehavior objBehavior in objBehaviorsInInventory)
        {
            if (objBehavior.obj == obj && objBehavior.gameObject.activeSelf)
                return true;
        }
        return false;
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
