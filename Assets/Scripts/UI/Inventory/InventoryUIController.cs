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

    public ScrollRect scrollRect;

    [HideInInspector]
    public bool pointerIn = false;

    public delegate void InventoryHoverEvent(GameObject go, PointingResult pointingResult);
    public static event InventoryHoverEvent OnCursorEnter;

    public delegate void InventoryClickEvent();
    public static event InventoryClickEvent OnClick;

    Coroutine showingCoroutine;

    private GeneralUIController generalUIController;
    public GeneralUIController GeneralUIController
    {
        get
        {
            if (generalUIController == null) generalUIController = GeneralUIController.instance;
            return generalUIController;
        }
    }

    private PCInventoryController inventoryController;
    public PCInventoryController InventoryController
    {
        get
        {
            if (inventoryController == null) inventoryController = PCController.instance.InventoryController;
            return inventoryController;
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

    public void ResetInventoryUI()
    {
        if(objCells != null)
        {
            for (int i = objCells.Count - 1; i >= 0; i--)
            {
                Destroy(objCells[i]);
            }
            objCells.Clear();
        }

        for(int i = objectsPanel.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(objectsPanel.transform.GetChild(i).gameObject);
        }
    }

    public void ShowUnshow(bool show)
    {
        if (showingCoroutine != null) return;
        if(show && !GeneralUIController.displayingInventoryUI)
        {
            scrollRect.verticalNormalizedPosition = 1;
            showingCoroutine = StartCoroutine(ShowUnshowCoroutine(unshowingPosition.position, showingPosition.position, 0.25f, show));
        }
        else if(!show && GeneralUIController.displayingInventoryUI)
        {
            showingCoroutine = StartCoroutine(ShowUnshowCoroutine(showingPosition.position, unshowingPosition.position, 0.25f, show));
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

            GeneralUIController.CurrentUI &= ~DisplayedUI.Inventory;
        }
        else
        {
            GeneralUIController.CurrentUI |= DisplayedUI.Inventory;
        }

        showingCoroutine = null;
    }

    public void AddObjCell(PickableObjBehavior objBehavior)
    {
        GameObject objCell = Instantiate(objectCellReference);

        objCells.Add(objCell);

        objCell.GetComponent<InventoryUIElement>().InitializeElement(this, objBehavior, objectsPanel.transform, objBehavior.GetInventorySprite());
    }

    public void RemoveObjCell(PickableObjBehavior objBehavior)
    {
        for(int i = 0; i < objCells.Count; i++)
        {
            InventoryUIElement element = objCells[i].GetComponent<InventoryUIElement>();
            if(element.objBehavior.obj == objBehavior.obj)
            {
                GameObject objCell = objCells[i];
                objCells.RemoveAt(i);
                Destroy(objCell);
                break;
            }
        }
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
