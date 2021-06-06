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

    public DataManager DataManager { get { return DataManager.Instance; } }

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

        foreach(PickableObjBehavior behavior in objBehaviorsInInventory)
        {
            behavior.InitializeObjBehavior(null);
        }

        DataManager.OnSaveData += SaveInventoryData;
        InventoryUIController.InitializeInventoryUI(objBehaviorsInInventory);
    }

    public PickableObjBehavior GetInventoryObj(InteractableObj obj)
    {
        foreach(PickableObjBehavior objBehavior in objBehaviorsInInventory)
        {
            if (objBehavior.obj == obj)
                return objBehavior;
        }

        return null;
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

                    behavior.LoadData(pickableObjData);
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
                PickableObjData objData = (PickableObjData)behavior.GetObjData();

                if (inventoryData.pickableObjInInventoryDatas.ContainsKey(behavior.obj.objID))
                    inventoryData.pickableObjInInventoryDatas[behavior.obj.objID] = objData;
                else
                    inventoryData.pickableObjInInventoryDatas.Add(behavior.obj.objID, objData);
            }
        }

        DataManager.SetInventoryData(inventoryData);
    }

    public void AddItemToInventory(List<InteractableObj> objs)
    {
        m_PCController.PlayPickSound();

        foreach (InteractableObj obj in objs)
        {
            if (obj is FabricObj fabricObj)
                AddItemToInventory(fabricObj);
            else
                AddItemToInventory(obj);
        }
    }

    void AddItemToInventory(InteractableObj obj)
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

    void AddItemToInventory(FabricObj obj)
    {
        foreach(PickableObjBehavior objBehaviorInInventory in objBehaviorsInInventory)
        {
            if(objBehaviorInInventory is FabricObjBehavior fabricBehavior && fabricBehavior.obj == obj)
            {
                fabricBehavior.gameObject.SetActive(true);
                fabricBehavior.inScene = true;
                fabricBehavior.color = obj.color;

                InventoryUIController.AddObjCell(fabricBehavior);
                break;
            }
        }
    }

    public void RemoveItemFromInventory(InteractableObj obj)
    {
        foreach (PickableObjBehavior objBehaviorInInventory in objBehaviorsInInventory)
        {
            if (objBehaviorInInventory.obj == obj)
            {
                objBehaviorInInventory.inScene = false;

                InventoryUIController.RemoveObjCell(objBehaviorInInventory);

                objBehaviorInInventory.gameObject.SetActive(false);
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

    public bool HasThreeFabrics()
    {
        int nFabrics = 0;
        foreach(PickableObjBehavior objBehavior in objBehaviorsInInventory)
        {
            if(objBehavior.gameObject.activeSelf && objBehavior is FabricObjBehavior)
            {
                nFabrics++;
            }
        }
        return nFabrics == 3;
    }

    public List<FabricObjBehavior> GetFabrics()
    {
        List<FabricObjBehavior> result = new List<FabricObjBehavior>();
        foreach(PickableObjBehavior objBehavior in objBehaviorsInInventory)
        {
            if (objBehavior.gameObject.activeSelf && objBehavior is FabricObjBehavior fabricObjBehavior)
                result.Add(fabricObjBehavior);
        }

        return result;
    }

    public void OpenInventory()
    {
        GeneralUIController.ShowInventoryUI();
        CameraManager.LockUnlockCurrentDetailCamera(false);
    }

    public void CloseInventory()
    {
        GeneralUIController.UnshowInventoryUI();
        CameraManager.LockUnlockCurrentDetailCamera(true);
    }
}
