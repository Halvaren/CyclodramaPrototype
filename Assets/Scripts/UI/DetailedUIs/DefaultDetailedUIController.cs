using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Manages the default detailed UI
/// </summary>
public class DefaultDetailedUIController : DetailedUIBase
{
    public TextMeshProUGUI objectText;

    /// <summary>
    /// Sets the behavior reference and the name of the object to display
    /// </summary>
    /// <param name="behavior"></param>
    /// <param name="initialText"></param>
    public void InitializeUI(DetailedObjBehavior behavior, string initialText)
    {
        this.behavior = behavior;
        objectText.text = char.ToUpper(initialText[0]) + initialText.Substring(1);
    }
}
