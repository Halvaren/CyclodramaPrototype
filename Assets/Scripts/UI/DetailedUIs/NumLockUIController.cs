using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NumLockUIController : DetailedUIBase
{
    protected NumLockObjBehavior numLockObjBehavior;
    public NumLockObjBehavior NumLockObjBehavior
    {
        get
        {
            if(numLockObjBehavior == null)
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
    public Button getBackButton;

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

    public void OnClickOpen()
    {
        NumLockObjBehavior.Open();
    }

    public void OnClickedInspect()
    {
        NumLockObjBehavior.Inspect();
    }

    public override void BlockInput(bool value)
    {
        foreach(Button wheelButton in wheelButtons)
        {
            wheelButton.interactable = !value;
        }

        openButton.interactable = !value;
        inspectButton.interactable = !value;
        getBackButton.interactable = !value;
    }
}
