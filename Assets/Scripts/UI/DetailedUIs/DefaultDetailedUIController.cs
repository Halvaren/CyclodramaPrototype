using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DefaultDetailedUIController : DetailedUIBase
{
    public TextMeshProUGUI objectText;

    public void InitializeUI(DetailedObjBehavior behavior, string initialText)
    {
        this.behavior = behavior;
        objectText.text = char.ToUpper(initialText[0]) + initialText.Substring(1);
    }
}
