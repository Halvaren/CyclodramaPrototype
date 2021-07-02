using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the UI of a numerical lock
/// </summary>
public class NumLockUIController : DetailedUIBase
{
    protected NumLockObjBehavior numLockObjBehavior;
    public NumLockObjBehavior NumLockObjBehavior
    {
        get
        {
            if(numLockObjBehavior == null || numLockObjBehavior != behavior)
            {
                if(behavior is NumLockObjBehavior numLockObjBehavior)
                {
                    this.numLockObjBehavior = numLockObjBehavior;
                }
                else
                {
                    Debug.LogError("It can't be casted");
                }
            }
            return numLockObjBehavior;
        }
    }

    public List<Button> wheelButtons;
    public Button openButton;
    public Button inspectButton;

    public GameObject[] arrows;

    /// <summary>
    /// Initializes the UI
    /// </summary>
    /// <param name="behavior"></param>
    public void InitializeUI(NumLockObjBehavior behavior)
    {
        this.behavior = behavior;
    }

    /// <summary>
    /// Coroutine that shows or unshows the UI
    /// </summary>
    /// <param name="initialPos"></param>
    /// <param name="finalPos"></param>
    /// <param name="time"></param>
    /// <param name="show"></param>
    /// <returns></returns>
    public override IEnumerator ShowUnshowCoroutine(Vector3 initialPos, Vector3 finalPos, float time, bool show)
    {
        if(!show)
        {
            foreach(GameObject arrow in arrows)
            {
                arrow.SetActive(false);
            }
        }

        yield return base.ShowUnshowCoroutine(initialPos, finalPos, time, show);

        if(show)
        {
            foreach(GameObject arrow in arrows)
            {
                arrow.SetActive(true);
            }
        }
    }

    /// <summary>
    /// It is executed when any of the arrows is clicked
    /// </summary>
    /// <param name="buttonIndex"></param>
    public void OnClickArrowButton(int buttonIndex)
    {
        switch(buttonIndex)
        {
            case 0:
                NumLockObjBehavior.TurnWheel(NumLockWheel.Left, true);
                break;
            case 1:
                NumLockObjBehavior.TurnWheel(NumLockWheel.Center, true);
                break;
            case 2:
                NumLockObjBehavior.TurnWheel(NumLockWheel.Right, true);
                break;
            case 3:
                NumLockObjBehavior.TurnWheel(NumLockWheel.Left, false);
                break;
            case 4:
                NumLockObjBehavior.TurnWheel(NumLockWheel.Center, false);
                break;
            case 5:
                NumLockObjBehavior.TurnWheel(NumLockWheel.Right, false);
                break;
        }
    }

    /// <summary>
    /// It is executed when open button is clicked
    /// </summary>
    public void OnClickOpen()
    {
        NumLockObjBehavior.Open();
    }

    /// <summary>
    /// It is executed when inspect button is clicked
    /// </summary>
    public void OnClickedInspect()
    {
        NumLockObjBehavior.Inspect();
    }

    /// <summary>
    /// Activates or deactivates the interaction of the buttons of the UI
    /// </summary>
    /// <param name="value"></param>
    public override void BlockInput(bool value)
    {
        foreach(Button wheelButton in wheelButtons)
        {
            wheelButton.interactable = !value;
        }

        openButton.interactable = !value;
        inspectButton.interactable = !value;
    }
}
