using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlembicAnimatorListener : MonoBehaviour
{
    [HideInInspector]
    public InteractableObjBehavior behavior;

    public void ExecuteAnimationCallback()
    {
        behavior?.ExecuteAnimationCallback();
    }

    public void ExecuteSecondAnimationCallback()
    {
        behavior?.ExecuteSecondAnimationCallback();
    }
}
