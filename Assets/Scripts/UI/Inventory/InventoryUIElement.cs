using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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

    public void OnPointerEnter(PointerEventData eventData)
    {
        inventoryUIController.OnPointerEnter(objBehavior.gameObject);
        Image.sprite = highlightedFrameSprite;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        inventoryUIController.OnPointerExit();
        Image.sprite = unhighlightedFrameSprite;
    }

    public void OnClick()
    {
        inventoryUIController.OnClickInventoryItem();
    }
}
