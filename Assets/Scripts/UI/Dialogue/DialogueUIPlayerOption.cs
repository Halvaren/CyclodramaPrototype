using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Each of the options that player can choose in a dialogue with an NPC
/// </summary>
public class DialogueUIPlayerOption : MonoBehaviour, IPointerEnterHandler
{
    public int commentIndex;
    public Button button;
    public Image buttonImage;
    public Sprite highlightedSprite;
    public Sprite unhighlightedSprite;
    public TextMeshProUGUI playerOptionText;

    public DialogueUIController dialogueUIController;

    public int optionIndex = -1;

    /// <summary>
    /// It is executed when the option is clicked
    /// </summary>
    public void OnClickButton()
    {
        dialogueUIController.OnClickPlayerOption(optionIndex);
    }

    /// <summary>
    /// Highlights or unhighlights the option
    /// </summary>
    /// <param name="value"></param>
    public void Highlight(bool value)
    {
        buttonImage.sprite = value ? highlightedSprite : unhighlightedSprite;
        if ((highlightedSprite == null && value) || (unhighlightedSprite == null && !value))
            buttonImage.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        else if ((highlightedSprite != null && value) || (unhighlightedSprite != null && !value))
            buttonImage.color = Color.white;
    }

    /// <summary>
    /// It is executed when the pointer enters the object
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        dialogueUIController.OnHoverPlayerOption(optionIndex);
    }
}
