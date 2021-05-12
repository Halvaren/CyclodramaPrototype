using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DialogueUIPlayerOption : MonoBehaviour, IPointerEnterHandler
{
    public Button button;
    public Image buttonImage;
    public Sprite highlightedSprite;
    public Sprite unhighlightedSprite;
    public TextMeshProUGUI playerOptionText;

    public DialogueUIController dialogueUIController;

    public int optionIndex = -1;

    public void OnClickButton()
    {
        dialogueUIController.OnClickPlayerOption(optionIndex);
    }

    public void Highlight(bool value)
    {
        buttonImage.sprite = value ? highlightedSprite : unhighlightedSprite;
        if ((highlightedSprite == null && value) || (unhighlightedSprite == null && !value))
            buttonImage.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        else if ((highlightedSprite != null && value) || (unhighlightedSprite != null && !value))
            buttonImage.color = Color.white;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        dialogueUIController.OnHoverPlayerOption(optionIndex);
    }
}
