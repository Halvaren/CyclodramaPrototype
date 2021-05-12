using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollBarHandleBehavior : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Scrollbar scrollbar;

    public Image imageHandler;
    public Sprite highlightedSprite;
    public Sprite unhighlightedSprite;

    public void OnPointerEnter(PointerEventData eventData)
    {
        imageHandler.sprite = highlightedSprite;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        imageHandler.sprite = unhighlightedSprite;
    }

    public void OnUpArrowPressed()
    {
        scrollbar.value += 0.1f;
    }

    public void OnDownArrowPressed()
    {
        scrollbar.value -= 0.1f;
    }
}
