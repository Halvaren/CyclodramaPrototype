using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Each of the object cells shown in the inventory
/// </summary>
public class InventoryUIElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private InventoryUIController inventoryUIController;
    public PickableObjBehavior objBehavior;

    public Image objectImage;
    public Sprite unhighlightedFrameSprite;
    public Sprite highlightedFrameSprite;

    private Image image;
    public Image Image
    {
        get
        {
            if (image == null) image = GetComponent<Image>();
            return image;
        }
    }

    private Button button;
    public Button Button
    {
        get
        {
            if (button == null) button = GetComponent<Button>();
            return button;
        }
    }

    /// <summary>
    /// Initializes the object cell
    /// </summary>
    /// <param name="inventoryUIController"></param>
    /// <param name="objBehavior"></param>
    /// <param name="parent"></param>
    /// <param name="sprite"></param>
    public void InitializeElement(InventoryUIController inventoryUIController, PickableObjBehavior objBehavior, Transform parent, Sprite sprite)
    {
        this.inventoryUIController = inventoryUIController;
        this.objBehavior = objBehavior;
        transform.SetParent(parent, false);

        GetComponent<RectTransform>().localScale = Vector3.one;
        objectImage.sprite = sprite;
        Image.sprite = unhighlightedFrameSprite;

        gameObject.SetActive(true);
    }

    /// <summary>
    /// It is executed when the pointer enters the object cell
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        inventoryUIController.OnPointerEnter(objBehavior.gameObject);
        Image.sprite = highlightedFrameSprite;
    }

    /// <summary>
    /// It is executed when the pointer exits the object cell
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerExit(PointerEventData eventData)
    {
        inventoryUIController.OnPointerExit();
        Image.sprite = unhighlightedFrameSprite;
    }

    /// <summary>
    /// It is executed when the object cell is clicked
    /// </summary>
    public void OnClick()
    {
        inventoryUIController.OnClickInventoryItem();
    }
}
