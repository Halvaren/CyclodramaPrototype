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

    public PCInventoryController InventoryController
    {
        get
        {
            return PCController.Instance.InventoryController;
        }
    }

    public void InitializeInventoryUI(int initialObjs)
    {
        inventoryContainer.SetActive(false);

        objCells = new List<GameObject>();
        for(int i = 0; i < initialObjs; i++)
        {
            AddObjCell();
        }
    }

    public void OpenCloseInventory()
    {
        if(inventoryContainer.activeSelf)
        {
            inventoryContainer.SetActive(false);

            PCController.Instance.EnableGameplayInput(true);
        }
        else
        {
            inventoryContainer.SetActive(true);

            PCController.Instance.EnableGameplayInput(false);
        }
    }

    public void AddObjCell()
    {
        GameObject objCell = Instantiate(objectCellReference);

        objCells.Add(objCell);
        int index = objCells.Count - 1;

        objCell.GetComponent<Button>().onClick.AddListener(delegate () { OnClickInventoryObj(index); });

        objCell.SetActive(true);
        objCell.transform.SetParent(objectsPanel.transform, false);
        objCell.GetComponent<RectTransform>().localScale = Vector3.one;
    }

    public void OnClickInventoryObj(int index)
    {
        InventoryController.InventoryItemClicked(index);
    }
}
