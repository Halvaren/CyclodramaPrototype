using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryUIElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private InventoryUIController inventoryUIController;
    private PickableObjBehavior objBehavior;

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

    public void InitializeElement(InventoryUIController inventoryUIController, PickableObjBehavior objBehavior, Transform parent, Sprite sprite, UnityAction listener)
    {
        this.inventoryUIController = inventoryUIController;
        this.objBehavior = objBehavior;
        transform.SetParent(parent, false);

        Button.onClick.AddListener(listener);
        GetComponent<RectTransform>().localScale = Vector3.one;
        Image.sprite = sprite;

        gameObject.SetActive(true);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        inventoryUIController.OnPointerEnter(objBehavior.gameObject);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        inventoryUIController.OnPointerExit();
    }
}
