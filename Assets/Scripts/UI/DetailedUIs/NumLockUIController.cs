using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NumLockUIController : DetailedUIBase
{
    [HideInInspector]
    public NumLockObjBehavior behavior;

    public List<Button> wheelButtons;
    public Button openButton;
    public Button inspectButton;
    public Button getBackButton;

    public void OnClickArrowButton(int buttonIndex)
    {
        switch(buttonIndex)
        {
            case 0:
                behavior.TurnWheel(NumLockWheel.Left, true);
                break;
            case 1:
                behavior.TurnWheel(NumLockWheel.Center, true);
                break;
            case 2:
                behavior.TurnWheel(NumLockWheel.Right, true);
                break;
            case 3:
                behavior.TurnWheel(NumLockWheel.Left, false);
                break;
            case 4:
                behavior.TurnWheel(NumLockWheel.Center, false);
                break;
            case 5:
                behavior.TurnWheel(NumLockWheel.Right, false);
                break;
        }
    }

    public void OnClickOpen()
    {
        behavior.Open();
    }

    public void OnClickedInspect()
    {
        behavior.Inspect();
    }

    public void OnClickBack()
    {
        behavior.GetBack();
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
