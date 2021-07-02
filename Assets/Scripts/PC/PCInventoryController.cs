using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PCComponent that manages PC inventory
/// </summary>
[CreateAssetMenu(menuName = "PCComponents/Inventory Controller")]
public class PCInventoryController : PCComponent
{
    #region Variables

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

    #endregion

    /// <summary>
    /// Initializes inventory: fills list of items, loads data, initializes items and initializes inventory UI
    /// </summary>
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

    /// <summary>
    /// Gets the pickableObjs that are children in hierarchy of this gameObject and adds it to the item list
    /// </summary>
    void GetInventoryObjs()
    {
        PickableObjBehavior[] objBehaviors = GetComponentsInChildren<PickableObjBehavior>(true);

        objBehaviorsInInventory = new List<PickableObjBehavior>();
        foreach(PickableObjBehavior behavior in objBehaviors)
        {
            objBehaviorsInInventory.Add(behavior);
        }
    }

    /// <summary>
    /// Reads the data from inventoryData and transfers it to each item in inventory
    /// </summary>
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

    /// <summary>
    /// Collects data from each item in inventory, stores in inventoryData and sends that object to DataManager
    /// </summary>
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
                objData.id = behavior.obj.objID;

                if (inventoryData.pickableObjInInventoryDatas.ContainsKey(behavior.obj.objID))
                    inventoryData.pickableObjInInventoryDatas[behavior.obj.objID] = objData;
                else
                    inventoryData.pickableObjInInventoryDatas.Add(behavior.obj.objID, objData);
            }
        }

        DataManager.SetInventoryData(inventoryData);
    }

    /// <summary>
    /// Adds a list of item to the inventory
    /// </summary>
    /// <param name="objs"></param>
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

    /// <summary>
    /// Adds a basic obj to the inventory
    /// </summary>
    /// <param name="obj"></param>
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

    /// <summary>
    /// Adds a fabric obj to the inventory
    /// </summary>
    /// <param name="obj"></param>
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

    /// <summary>
    /// Remove an obj from the inventory
    /// </summary>
    /// <param name="obj"></param>
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

    /// <summary>
    /// Checks if the given obj is inventory
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public bool IsItemInInventory(InteractableObj obj)
    {
        foreach(PickableObjBehavior objBehavior in objBehaviorsInInventory)
        {
            if (objBehavior.obj == obj && objBehavior.gameObject.activeSelf)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Returns the pickableObj that represents the given obj
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public PickableObjBehavior GetInventoryObj(InteractableObj obj)
    {
        foreach (PickableObjBehavior objBehavior in objBehaviorsInInventory)
        {
            if (objBehavior.obj == obj)
                return objBehavior;
        }

        return null;
    }

    /// <summary>
    /// Checks if there are three fabric objs in the inventory
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Returns the list of fabricObjBehaviors contained in the inventory
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Opens inventory menu
    /// </summary>
    public void OpenInventory()
    {
        GeneralUIController.ShowInventoryUI();
        CameraManager.LockUnlockCurrentDetailCamera(false);
    }

    /// <summary>
    /// Closes inventory menu
    /// </summary>
    public void CloseInventory()
    {
        GeneralUIController.UnshowInventoryUI();
        CameraManager.LockUnlockCurrentDetailCamera(true);
    }
}
