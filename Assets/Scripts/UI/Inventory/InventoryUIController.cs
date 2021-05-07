using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryUIController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject inventoryContainer;
    private RectTransform inventoryContainerRectTransform;
    public RectTransform InventoryContainerRectTransform
    {
        get
        {
            if (inventoryContainerRectTransform == null) inventoryContainerRectTransform = inventoryContainer.GetComponent<RectTransform>();
            return inventoryContainerRectTransform;
        }
    }

    public GameObject objectsPanel;
    public GameObject objectCellReference;

    [HideInInspector]
    public List<GameObject> objCells;

    public RectTransform showingPosition;
    public RectTransform unshowingPosition;

    [HideInInspector]
    public bool pointerIn = false;

    public delegate void InventoryHoverEvent(GameObject go, PointingResult pointingResult);
    public static event InventoryHoverEvent OnCursorEnter;

    public delegate void InventoryClickEvent();
    public static event InventoryClickEvent OnClick;

    public bool showingInventory
    {
        get { return inventoryContainer.activeSelf; }
    }

    private RectTransform rectTransform;
    public RectTransform RectTransform
    {
        get
        {
            if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
            return rectTransform;
        }
    }

    public PCInventoryController InventoryController
    {
        get
        {
            return PCController.instance.InventoryController;
        }
    }

    public void InitializeInventoryUI(List<PickableObjBehavior> initialObjs)
    {
        inventoryContainer.SetActive(false);
        InventoryContainerRectTransform.position = unshowingPosition.position;

        objCells = new List<GameObject>();
        for(int i = 0; i < initialObjs.Count; i++)
        {
            if(initialObjs[i].gameObject.activeSelf)
                AddObjCell(initialObjs[i]);
        }
    }

    public void ShowUnshow(bool show)
    {
        if(show && !showingInventory)
        { 
            StartCoroutine(ShowUnshowCoroutine(unshowingPosition.position, showingPosition.position, 0.25f, show));
        }
        else if(!show && showingInventory)
        {
            StartCoroutine(ShowUnshowCoroutine(showingPosition.position, unshowingPosition.position, 0.25f, show));
        }
    }

    IEnumerator ShowUnshowCoroutine(Vector3 initialPos, Vector3 finalPos, float time, bool show)
    {
        if (show)
        {
            inventoryContainer.SetActive(true);
            PCController.instance.EnableGameplayInput(false);
        }

        float elapsedTime = 0.0f;

        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;

            InventoryContainerRectTransform.position = Vector3.Lerp(initialPos, finalPos, elapsedTime / time);

            yield return null;
        }
        InventoryContainerRectTransform.position = finalPos;

        if (!show)
        {
            inventoryContainer.SetActive(false);
            PCController.instance.EnableGameplayInput(true);
        }
    }

    public void AddObjCell(PickableObjBehavior objBehavior)
    {
        GameObject objCell = Instantiate(objectCellReference);

        objCells.Add(objCell);

        objCell.GetComponent<InventoryUIElement>().InitializeElement(this, objBehavior, objectsPanel.transform, objBehavior.obj.inventorySprite);
    }

    public void OnClickInventoryItem()
    {
        OnClick();
    }

    public void OnPointerEnter(GameObject go)
    {
        OnCursorEnter(go, PointingResult.Object);
    }

    public void OnPointerExit()
    {
        OnCursorEnter(null, PointingResult.Nothing);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        pointerIn = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pointerIn = false;
    }
}
