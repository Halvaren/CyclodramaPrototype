using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetailedUIController : MonoBehaviour
{
    public NumLockUIController numLockUIController;

    public bool showingAnyDetailedUI
    {
        get { return numLockUIController.gameObject.activeSelf; }
    }

    private void Start()
    {
        numLockUIController.gameObject.SetActive(false);
    }

    public DetailedUIBase ShowUnshow(bool value, DetailedObjBehavior behavior = null)
    {
        if(value)
        {
            if(behavior is NumLockObjBehavior)
            {
                numLockUIController.gameObject.SetActive(true);
                numLockUIController.behavior = (NumLockObjBehavior)behavior;
                return numLockUIController;
            }
        }
        else
        {
            numLockUIController.gameObject.SetActive(false);
            numLockUIController.behavior = null;
        }
        return null;
    }
}

public class DetailedUIBase : MonoBehaviour
{
    public virtual void BlockInput(bool value)
    {

    }
}
