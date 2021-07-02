using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Manages any scroll bar used in UIs
/// </summary>
public class ScrollBarHandleBehavior : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Scrollbar scrollbar;

    public Image imageHandler;
    public Sprite highlightedSprite;
    public Sprite unhighlightedSprite;

    /// <summary>
    /// It is executed when pointer enters the scroll bar
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        imageHandler.sprite = highlightedSprite;
    }

    /// <summary>
    /// It is executed when pointer exits the scroll bar
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerExit(PointerEventData eventData)
    {
        imageHandler.sprite = unhighlightedSprite;
    }

    /// <summary>
    /// It is executed when the top arrow of the scroll bar is clicked
    /// </summary>
    public void OnUpArrowPressed()
    {
        scrollbar.value += 0.1f;
    }

    /// <summary>
    /// It is executed when the bottom arrow of the scroll bar is clicked
    /// </summary>
    public void OnDownArrowPressed()
    {
        scrollbar.value -= 0.1f;
    }
}
