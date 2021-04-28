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
    public TextMeshProUGUI playerOptionText;

    public DialogueUIController dialogueUIController;

    public int optionIndex = -1;

    public void OnClickButton()
    {
        dialogueUIController.OnClickPlayerOption(optionIndex);
    }

    public void Highlight(bool value)
    {
        buttonImage.color = value ? Color.yellow : Color.white;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        dialogueUIController.OnHoverPlayerOption(optionIndex);
    }
}
