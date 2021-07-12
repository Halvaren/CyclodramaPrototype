using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Manages the UI of the inventory
/// </summary>
public class InventoryUIController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    #region Variables

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

    public AudioClip openClip;
    public AudioClip closeClip;

    public AudioClip[] drawingClips;
    int drawingClipPointer = 0;

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

    #endregion

    /// <summary>
    /// Initializes the UI: creates as many item cells as elements of the list passed as a parameter
    /// </summary>
    /// <param name="initialObjs"></param>
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

    /// <summary>
    /// Destroys all item cells
    /// </summary>
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

    /// <summary>
    /// Shows or unshows the UI
    /// </summary>
    /// <param name="show"></param>
    /// <returns>Returns true if the UI is now showing or hiding</returns>
    public bool ShowUnshow(bool show)
    {
        if (showingCoroutine != null) return false;
        if(show && !GeneralUIController.displayingInventoryUI)
        {
            GeneralUIController.PlayUISound(openClip);
            scrollRect.verticalNormalizedPosition = 1;
            showingCoroutine = StartCoroutine(ShowUnshowCoroutine(unshowingPosition.position, showingPosition.position, 0.25f, show));
        }
        else if(!show && GeneralUIController.displayingInventoryUI)
        {
            GeneralUIController.PlayUISound(closeClip);
            showingCoroutine = StartCoroutine(ShowUnshowCoroutine(showingPosition.position, unshowingPosition.position, 0.25f, show));
        }

        return true;
    }

    /// <summary>
    /// Coroutine that shows or unshows the UI
    /// </summary>
    /// <param name="initialPos"></param>
    /// <param name="finalPos"></param>
    /// <param name="time"></param>
    /// <param name="show"></param>
    /// <returns></returns>
    IEnumerator ShowUnshowCoroutine(Vector3 initialPos, Vector3 finalPos, float time, bool show)
    {
        if (show)
        {
            inventoryContainer.SetActive(true);
            PCController.instance.EnableGameplayInput(false, false);
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
            PCController.instance.EnableGameplayInput(true, false);

            GeneralUIController.CurrentUI &= ~DisplayedUI.Inventory;
        }
        else
        {
            GeneralUIController.CurrentUI |= DisplayedUI.Inventory;
        }

        showingCoroutine = null;
    }

    /// <summary>
    /// Adds a new object cell because one item has been added to the inventory
    /// </summary>
    /// <param name="objBehavior"></param>
    public void AddObjCell(PickableObjBehavior objBehavior)
    {
        GameObject objCell = Instantiate(objectCellReference);

        objCells.Add(objCell);

        objCell.GetComponent<InventoryUIElement>().InitializeElement(this, objBehavior, objectsPanel.transform, objBehavior.GetInventorySprite());
    }

    /// <summary>
    /// Removes an object cell because one item has been consumed
    /// </summary>
    /// <param name="objBehavior"></param>
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

    /// <summary>
    /// It is executed when an inventory item is clicked
    /// </summary>
    public void OnClickInventoryItem()
    {
        OnClick();
    }

    /// <summary>
    /// It is executed when the pointer enters an inventory intem
    /// </summary>
    /// <param name="go"></param>
    public void OnPointerEnter(GameObject go)
    {
        UpdateDrawingClipPointer();

        GeneralUIController.PlayUISound(drawingClips[drawingClipPointer]);
        OnCursorEnter(go, PointingResult.Object);
    }

    /// <summary>
    /// It is executed when the pointer exits an inventory item
    /// </summary>
    public void OnPointerExit()
    {
        OnCursorEnter(null, PointingResult.Nothing);
    }

    /// <summary>
    /// It is executed when the pointer enters the inventory background object
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        pointerIn = true;
    }

    /// <summary>
    /// It is executed when the pointer exits the inventory background object
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerExit(PointerEventData eventData)
    {
        pointerIn = false;
    }

    /// <summary>
    /// Updates the pointer that randomly indicates which drawing sound must be played next time
    /// </summary>
    void UpdateDrawingClipPointer()
    {
        int randNum = UnityEngine.Random.Range(0, drawingClips.Length);
        if (randNum == drawingClipPointer)
        {
            drawingClipPointer += (int)Mathf.Pow(-1, UnityEngine.Random.Range(0, 1));

            if (drawingClipPointer < 0) drawingClipPointer = drawingClips.Length - 1;
            else if (drawingClipPointer >= drawingClips.Length) drawingClipPointer = 0;
        }
        else drawingClipPointer = randNum;
    }
}
