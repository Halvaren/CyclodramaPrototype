using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    public void InitializeUI(NumLockObjBehavior behavior)
    {
        this.behavior = behavior;
    }

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
    }
}
