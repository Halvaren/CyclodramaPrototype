using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetailedUIController : MonoBehaviour
{
    public NumLockUIController numLockUIController;
    public DefaultDetailedUIController defaultDetailedUIController;

    private void Start()
    {
        numLockUIController.gameObject.SetActive(false);
        defaultDetailedUIController.gameObject.SetActive(false);
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

            defaultDetailedUIController.gameObject.SetActive(true);
            defaultDetailedUIController.behavior = behavior;
            return defaultDetailedUIController;
        }
        else
        {
            numLockUIController.gameObject.SetActive(false);
            numLockUIController.behavior = null;

            defaultDetailedUIController.gameObject.SetActive(false);
            defaultDetailedUIController.behavior = null;
        }
        return null;
    }
}

public class DetailedUIBase : MonoBehaviour
{
    [HideInInspector]
    public DetailedObjBehavior behavior;

    public AudioClip openClip;
    public AudioClip closeClip;

    public virtual void BlockInput(bool value)
    {

    }

    public void OnClickBack()
    {
        behavior.GetBack();
    }
}
