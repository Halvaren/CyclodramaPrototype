using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUIController : MonoBehaviour
{
    public GameObject inventoryContainer;

    public GameObject objectsPanel;
    public GameObject objectCellReference;
    public Scrollbar scrollbar;

    public List<GameObject> objCells;

    public delegate void InventoryHoverEvent(GameObject go, PointingResult pointingResult);
    public static event InventoryHoverEvent OnCursorEnter;

    public PCInventoryController InventoryController
    {
        get
        {
            return PCController.Instance.InventoryController;
        }
    }

    public void InitializeInventoryUI(List<PickableObjBehavior> initialObjs)
    {
        inventoryContainer.SetActive(false);

        objCells = new List<GameObject>();
        for(int i = 0; i < initialObjs.Count; i++)
        {
            AddObjCell(initialObjs[i]);
        }
    }

    public bool OpenCloseInventory()
    {
        if(inventoryContainer.activeSelf)
        {
            inventoryContainer.SetActive(false);

            PCController.Instance.EnableGameplayInput(true);

            return false;
        }
        else
        {
            inventoryContainer.SetActive(true);

            PCController.Instance.EnableGameplayInput(false);

            return true;
        }
    }

    public void AddObjCell(PickableObjBehavior objBehavior)
    {
        GameObject objCell = Instantiate(objectCellReference);

        objCells.Add(objCell);
        int index = objCells.Count - 1;

        objCell.GetComponent<InventoryUIElement>().InitializeElement(this, objBehavior, objectsPanel.transform, objBehavior.obj.inventorySprite, delegate () { OnClickInventoryObj(index); });
    }

    public void OnClickInventoryObj(int index)
    {
        InventoryController.InventoryItemClicked(index);
    }

    public void OnPointerEnter(GameObject go)
    {
        OnCursorEnter(go, PointingResult.Object);
    }

    public void OnPointerExit()
    {
        OnCursorEnter(null, PointingResult.Nothing);
    }
}
